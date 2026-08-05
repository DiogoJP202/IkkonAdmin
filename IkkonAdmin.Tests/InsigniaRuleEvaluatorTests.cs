using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class InsigniaRuleEvaluatorTests
{
    [Theory]
    [InlineData("FREQUENCIA_PRIMEIRA")]
    [InlineData("FREQUENCIA_TOTAL:10")]
    [InlineData("TEMPO_ATIVO_MESES:12")]
    public void ValidateRule_RegrasSuportadas_Aceita(string rule)
    {
        using var dbContext = CreateDbContext();
        var evaluator = new InsigniaRuleEvaluator(dbContext, new TestClock());

        Assert.True(evaluator.ValidateRule(rule).Success);
    }

    [Theory]
    [InlineData("FREQUENCIA_TOTAL:0")]
    [InlineData("TEMPO_ATIVO_MESES:x")]
    [InlineData("REGRA_ANTIGA")]
    public void ValidateRule_RegraDesconhecida_Rejeita(string rule)
    {
        using var dbContext = CreateDbContext();
        var evaluator = new InsigniaRuleEvaluator(dbContext, new TestClock());

        Assert.False(evaluator.ValidateRule(rule).Success);
    }

    [Fact]
    public async Task EvaluateAsync_FrequenciaPrimeira_ConcedeSemDuplicar()
    {
        await using var dbContext = CreateDbContext();
        var student = CreateStudent("Primeira frequência", new DateOnly(2026, 1, 1));
        var badge = new Insignia { Nome = "Primeira", Ativa = true, RegraAutomatica = "FREQUENCIA_PRIMEIRA" };
        var classroom = new Turma { Nome = "Turma", Modalidade = "Taiko", Ativa = true };
        var lesson = new Aula
        {
            Turma = classroom,
            Inicio = new DateTime(2026, 7, 1, 19, 0, 0),
            Fim = new DateTime(2026, 7, 1, 20, 0, 0)
        };
        dbContext.AddRange(student, badge, classroom, lesson);
        await dbContext.SaveChangesAsync();
        dbContext.FrequenciasAlunos.Add(new FrequenciaAluno
        {
            AlunoId = student.Id,
            AulaId = lesson.Id,
            Status = StatusFrequenciaEnum.Presente
        });
        await dbContext.SaveChangesAsync();
        var evaluator = new InsigniaRuleEvaluator(dbContext, new TestClock());

        var first = await evaluator.EvaluateAsync();
        var second = await evaluator.EvaluateAsync();

        Assert.Equal(1, first.AchievementsGranted);
        Assert.Equal(0, second.AchievementsGranted);
        Assert.Equal(1, second.AlreadyExisting);
        var achievement = await dbContext.AlunoInsignias.SingleAsync();
        Assert.Equal(InsigniaOrigemEnum.Automatica, achievement.Origem);
    }

    [Fact]
    public async Task EvaluateAsync_TotalFrequenciaETempoAtivo_ConcedeElegiveis()
    {
        await using var dbContext = CreateDbContext();
        var student = CreateStudent("Aluno elegível", new DateOnly(2025, 1, 1));
        var attendanceBadge = new Insignia { Nome = "Assíduo", Ativa = true, RegraAutomatica = "FREQUENCIA_TOTAL:2" };
        var tenureBadge = new Insignia { Nome = "Veterano", Ativa = true, RegraAutomatica = "TEMPO_ATIVO_MESES:12" };
        var classroom = new Turma { Nome = "Turma", Modalidade = "Taiko", Ativa = true };
        var firstLesson = CreateLesson(classroom, new DateTime(2026, 7, 1, 19, 0, 0));
        var secondLesson = CreateLesson(classroom, new DateTime(2026, 7, 8, 19, 0, 0));
        dbContext.AddRange(student, attendanceBadge, tenureBadge, classroom, firstLesson, secondLesson);
        await dbContext.SaveChangesAsync();
        dbContext.FrequenciasAlunos.AddRange(
            new FrequenciaAluno { AlunoId = student.Id, AulaId = firstLesson.Id },
            new FrequenciaAluno { AlunoId = student.Id, AulaId = secondLesson.Id });
        await dbContext.SaveChangesAsync();

        var summary = await new InsigniaRuleEvaluator(dbContext, new TestClock()).EvaluateAsync();

        Assert.Equal(2, summary.AchievementsGranted);
        Assert.Equal(2, await dbContext.AlunoInsignias.CountAsync());
    }

    [Fact]
    public async Task EvaluateAsync_RegraAntigaDesconhecida_AvisaENaoExecuta()
    {
        await using var dbContext = CreateDbContext();
        dbContext.AddRange(
            CreateStudent("Aluno", new DateOnly(2020, 1, 1)),
            new Insignia { Nome = "Legada", Ativa = true, RegraAutomatica = "REGRA_LIVRE_ANTIGA" });
        await dbContext.SaveChangesAsync();

        var summary = await new InsigniaRuleEvaluator(dbContext, new TestClock()).EvaluateAsync();

        Assert.Equal(0, summary.AchievementsGranted);
        Assert.Single(summary.InvalidRules);
        Assert.Empty(dbContext.AlunoInsignias);
    }

    private static Aluno CreateStudent(string name, DateOnly entryDate)
    {
        return new Aluno
        {
            NomeCompleto = name,
            CPF = Guid.NewGuid().ToString("N")[..11],
            DataEntrada = entryDate,
            Status = StatusAlunoEnum.Ativo
        };
    }

    private static Aula CreateLesson(Turma classroom, DateTime start)
    {
        return new Aula { Turma = classroom, Inicio = start, Fim = start.AddHours(1) };
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow => new(2026, 7, 13, 15, 0, 0, DateTimeKind.Utc);
        public DateTime Now => new(2026, 7, 13, 12, 0, 0);
        public DateTime Today => Now.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
