using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class InsigniaRuleEvaluator(
    ApplicationDbContext dbContext,
    IClock clock) : IInsigniaRuleEvaluator
{
    private const string FirstAttendanceRule = "FREQUENCIA_PRIMEIRA";
    private const string AttendanceTotalPrefix = "FREQUENCIA_TOTAL:";
    private const string ActiveMonthsPrefix = "TEMPO_ATIVO_MESES:";

    public OperationResult ValidateRule(string? rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
        {
            return OperationResult.Ok("Regra automática não informada.");
        }

        return TryParseRule(rule, out _)
            ? OperationResult.Ok("Regra automática válida.")
            : OperationResult.Fail(
                "Regra automática inválida. Use FREQUENCIA_PRIMEIRA, FREQUENCIA_TOTAL:n ou TEMPO_ATIVO_MESES:n.",
                nameof(Insignia.RegraAutomatica));
    }

    public async Task<InsigniaProcessingSummary> EvaluateAsync(
        IReadOnlyCollection<int>? studentIds = null,
        CancellationToken cancellationToken = default)
    {
        var rules = await dbContext.Insignias
            .AsNoTracking()
            .Where(x => x.Ativa && x.RegraAutomatica != null && x.RegraAutomatica != string.Empty)
            .OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.RegraAutomatica })
            .ToListAsync(cancellationToken);

        var restrictedStudentIds = studentIds is { Count: > 0 }
            ? studentIds.Distinct().ToArray()
            : null;
        var invalidRules = new List<string>();
        var granted = 0;
        var alreadyExisting = 0;
        var evaluated = 0;

        foreach (var insignia in rules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!TryParseRule(insignia.RegraAutomatica!, out var parsedRule))
            {
                invalidRules.Add(insignia.RegraAutomatica!);
                continue;
            }

            evaluated++;
            var eligibleStudentIds = await GetEligibleStudentIdsAsync(
                parsedRule,
                restrictedStudentIds,
                cancellationToken);
            if (eligibleStudentIds.Count == 0)
            {
                continue;
            }

            var existingStudentIds = await dbContext.AlunoInsignias
                .AsNoTracking()
                .Where(x => x.InsigniaId == insignia.Id && eligibleStudentIds.Contains(x.AlunoId))
                .Select(x => x.AlunoId)
                .ToListAsync(cancellationToken);
            var existingSet = existingStudentIds.ToHashSet();
            alreadyExisting += existingSet.Count;

            foreach (var studentId in eligibleStudentIds.Where(x => !existingSet.Contains(x)))
            {
                var achievement = new AlunoInsignia
                {
                    AlunoId = studentId,
                    InsigniaId = insignia.Id,
                    Origem = InsigniaOrigemEnum.Automatica,
                    ConcedidaEmUtc = clock.UtcNow,
                    Observacao = $"Regra automática: {insignia.RegraAutomatica}"
                };

                dbContext.AlunoInsignias.Add(achievement);
                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    granted++;
                    existingSet.Add(studentId);
                }
                catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
                {
                    dbContext.Entry(achievement).State = EntityState.Detached;
                    alreadyExisting++;
                    existingSet.Add(studentId);
                }
            }
        }

        return new InsigniaProcessingSummary(evaluated, granted, alreadyExisting, invalidRules);
    }

    private async Task<List<int>> GetEligibleStudentIdsAsync(
        ParsedRule rule,
        int[]? restrictedStudentIds,
        CancellationToken cancellationToken)
    {
        IQueryable<Aluno> students = dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Status == StatusAlunoEnum.Ativo);

        if (restrictedStudentIds is not null)
        {
            students = students.Where(x => restrictedStudentIds.Contains(x.Id));
        }

        return rule.Type switch
        {
            InsigniaRuleType.FirstAttendance => await students
                .Where(x => x.Frequencias.Any())
                .Select(x => x.Id)
                .ToListAsync(cancellationToken),
            InsigniaRuleType.AttendanceTotal => await students
                .Where(x => x.Frequencias.Count >= rule.Threshold)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken),
            InsigniaRuleType.ActiveMonths => await students
                .Where(x => x.DataEntrada <= SaoPauloTime.Today(clock.UtcNow).AddMonths(-rule.Threshold))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken),
            _ => []
        };
    }

    private static bool TryParseRule(string rule, out ParsedRule parsedRule)
    {
        var normalized = rule.Trim().ToUpperInvariant();
        if (normalized == FirstAttendanceRule)
        {
            parsedRule = new ParsedRule(InsigniaRuleType.FirstAttendance, 1);
            return true;
        }

        if (TryParseThreshold(normalized, AttendanceTotalPrefix, 100_000, out var attendanceThreshold))
        {
            parsedRule = new ParsedRule(InsigniaRuleType.AttendanceTotal, attendanceThreshold);
            return true;
        }

        if (TryParseThreshold(normalized, ActiveMonthsPrefix, 1_200, out var monthsThreshold))
        {
            parsedRule = new ParsedRule(InsigniaRuleType.ActiveMonths, monthsThreshold);
            return true;
        }

        parsedRule = default;
        return false;
    }

    private static bool TryParseThreshold(
        string rule,
        string prefix,
        int maximum,
        out int threshold)
    {
        threshold = 0;
        return rule.StartsWith(prefix, StringComparison.Ordinal) &&
               int.TryParse(rule[prefix.Length..], out threshold) &&
               threshold is > 0 &&
               threshold <= maximum;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 };
    }

    private readonly record struct ParsedRule(InsigniaRuleType Type, int Threshold);

    private enum InsigniaRuleType
    {
        FirstAttendance,
        AttendanceTotal,
        ActiveMonths
    }
}
