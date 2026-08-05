using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AulaRecurrenceGenerator(
    ApplicationDbContext dbContext,
    IClock clock,
    IConfiguracaoSistemaProvider configurationProvider) : IAulaRecurrenceGenerator
{
    public async Task<AulaGenerationSummary> GenerateAsync(
        DateOnly? startDate = null,
        int? horizonWeeks = null,
        CancellationToken cancellationToken = default)
    {
        var configuration = await configurationProvider.ObterOuCriarAsync(cancellationToken);
        var start = startDate ?? SaoPauloTime.Today(clock.UtcNow);
        var weeks = Math.Clamp(horizonWeeks ?? configuration.HorizonteGeracaoAulasSemanas, 1, 52);
        var endExclusive = start.AddDays(weeks * 7);

        var schedules = await dbContext.TurmaHorarios
            .AsNoTracking()
            .Where(x => x.Ativo && x.Turma != null && x.Turma.Ativa)
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var scheduleIds = schedules.Select(x => x.Id).ToArray();
        var existingOccurrences = scheduleIds.Length == 0
            ? new HashSet<(int ScheduleId, DateOnly Date)>()
            : (await dbContext.Aulas
                    .AsNoTracking()
                    .Where(x => x.TurmaHorarioId.HasValue &&
                                x.DataOcorrenciaRecorrencia.HasValue &&
                                scheduleIds.Contains(x.TurmaHorarioId.Value) &&
                                x.DataOcorrenciaRecorrencia.Value >= start &&
                                x.DataOcorrenciaRecorrencia.Value < endExclusive)
                    .Select(x => new { x.TurmaHorarioId, x.DataOcorrenciaRecorrencia })
                    .ToListAsync(cancellationToken))
                .Select(x => (x.TurmaHorarioId!.Value, x.DataOcorrenciaRecorrencia!.Value))
                .ToHashSet();

        var instructorLinks = await dbContext.TurmaInstrutores
            .AsNoTracking()
            .Where(x => x.Principal && x.UsuarioSistema != null && x.UsuarioSistema.Ativo)
            .OrderByDescending(x => x.DataInicio)
            .ToListAsync(cancellationToken);

        var occurrencesEvaluated = 0;
        var created = 0;
        var alreadyExisting = 0;
        var withoutInstructor = 0;

        foreach (var schedule in schedules)
        {
            foreach (var date in EnumerateDates(start, endExclusive, schedule.DiaSemana))
            {
                cancellationToken.ThrowIfCancellationRequested();
                occurrencesEvaluated++;

                var occurrenceKey = (schedule.Id, date);
                if (existingOccurrences.Contains(occurrenceKey))
                {
                    alreadyExisting++;
                    continue;
                }

                var instructorId = instructorLinks
                    .Where(x => x.TurmaId == schedule.TurmaId &&
                                x.DataInicio <= date &&
                                (!x.DataFim.HasValue || x.DataFim.Value >= date))
                    .OrderByDescending(x => x.DataInicio)
                    .Select(x => (int?)x.UsuarioSistemaId)
                    .FirstOrDefault();

                if (!instructorId.HasValue)
                {
                    withoutInstructor++;
                }

                var lesson = new Aula
                {
                    TurmaId = schedule.TurmaId,
                    TurmaHorarioId = schedule.Id,
                    DataOcorrenciaRecorrencia = date,
                    InstrutorUsuarioId = instructorId,
                    Inicio = date.ToDateTime(schedule.HoraInicio),
                    Fim = date.ToDateTime(schedule.HoraFim),
                    Local = schedule.Local,
                    Status = StatusAulaEnum.Agendada
                };

                dbContext.Aulas.Add(lesson);
                try
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                    existingOccurrences.Add(occurrenceKey);
                    created++;
                }
                catch (DbUpdateException exception) when (IsUniqueConstraintViolation(exception))
                {
                    dbContext.Entry(lesson).State = EntityState.Detached;
                    existingOccurrences.Add(occurrenceKey);
                    alreadyExisting++;
                }
            }
        }

        return new AulaGenerationSummary(
            schedules.Count,
            occurrencesEvaluated,
            created,
            alreadyExisting,
            withoutInstructor,
            start,
            endExclusive);
    }

    private static IEnumerable<DateOnly> EnumerateDates(
        DateOnly start,
        DateOnly endExclusive,
        DayOfWeek dayOfWeek)
    {
        var offset = ((int)dayOfWeek - (int)start.DayOfWeek + 7) % 7;
        for (var date = start.AddDays(offset); date < endExclusive; date = date.AddDays(7))
        {
            yield return date;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException { Number: 2601 or 2627 };
    }
}
