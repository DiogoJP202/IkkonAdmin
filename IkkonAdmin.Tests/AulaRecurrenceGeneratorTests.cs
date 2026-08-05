using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class AulaRecurrenceGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_Repetido_NaoDuplicaOcorrencias()
    {
        await using var dbContext = CreateDbContext();
        var schedule = await SeedScheduleAsync(dbContext, DayOfWeek.Monday);
        var generator = CreateGenerator(dbContext);

        var first = await generator.GenerateAsync(new DateOnly(2026, 7, 13), 2);
        var second = await generator.GenerateAsync(new DateOnly(2026, 7, 13), 2);

        Assert.Equal(2, first.Created);
        Assert.Equal(0, second.Created);
        Assert.Equal(2, second.AlreadyExisting);
        Assert.Equal(2, await dbContext.Aulas.CountAsync(x => x.TurmaHorarioId == schedule.Id));
    }

    [Fact]
    public async Task GenerateAsync_HorarioInativo_NaoGera()
    {
        await using var dbContext = CreateDbContext();
        await SeedScheduleAsync(dbContext, DayOfWeek.Monday, active: false);
        var generator = CreateGenerator(dbContext);

        var summary = await generator.GenerateAsync(new DateOnly(2026, 7, 13), 2);

        Assert.Equal(0, summary.SchedulesEvaluated);
        Assert.Empty(dbContext.Aulas);
    }

    [Fact]
    public async Task GenerateAsync_AulaCanceladaOuReagendada_PreservaOcorrenciaExistente()
    {
        await using var dbContext = CreateDbContext();
        var schedule = await SeedScheduleAsync(dbContext, DayOfWeek.Monday);
        var originalOccurrence = new DateOnly(2026, 7, 13);
        var rescheduledStart = new DateTime(2026, 7, 14, 20, 0, 0);
        var existing = new Aula
        {
            TurmaId = schedule.TurmaId,
            TurmaHorarioId = schedule.Id,
            DataOcorrenciaRecorrencia = originalOccurrence,
            Inicio = rescheduledStart,
            Fim = rescheduledStart.AddHours(1),
            Status = StatusAulaEnum.Cancelada
        };
        dbContext.Aulas.Add(existing);
        await dbContext.SaveChangesAsync();

        var summary = await CreateGenerator(dbContext).GenerateAsync(originalOccurrence, 1);

        Assert.Equal(0, summary.Created);
        Assert.Equal(1, summary.AlreadyExisting);
        var preserved = await dbContext.Aulas.SingleAsync();
        Assert.Equal(rescheduledStart, preserved.Inicio);
        Assert.Equal(StatusAulaEnum.Cancelada, preserved.Status);
    }

    [Fact]
    public async Task GenerateAsync_SelecionaInstrutorPrincipalAtivoNaData()
    {
        await using var dbContext = CreateDbContext();
        var schedule = await SeedScheduleAsync(dbContext, DayOfWeek.Monday);
        var activeInstructor = CreateInstructor("principal.ativo");
        var expiredInstructor = CreateInstructor("principal.expirado");
        dbContext.AddRange(activeInstructor, expiredInstructor);
        await dbContext.SaveChangesAsync();
        dbContext.TurmaInstrutores.AddRange(
            new TurmaInstrutor
            {
                TurmaId = schedule.TurmaId,
                UsuarioSistemaId = activeInstructor.Id,
                Principal = true,
                DataInicio = new DateOnly(2026, 7, 1)
            },
            new TurmaInstrutor
            {
                TurmaId = schedule.TurmaId,
                UsuarioSistemaId = expiredInstructor.Id,
                Principal = true,
                DataInicio = new DateOnly(2026, 1, 1),
                DataFim = new DateOnly(2026, 6, 30)
            });
        await dbContext.SaveChangesAsync();

        var summary = await CreateGenerator(dbContext).GenerateAsync(new DateOnly(2026, 7, 13), 1);

        Assert.Equal(1, summary.Created);
        Assert.Equal(activeInstructor.Id, (await dbContext.Aulas.SingleAsync()).InstrutorUsuarioId);
    }

    private static AulaRecurrenceGenerator CreateGenerator(ApplicationDbContext dbContext)
    {
        var clock = new TestClock();
        return new AulaRecurrenceGenerator(
            dbContext,
            clock,
            new ConfiguracaoSistemaProvider(dbContext, clock));
    }

    private static async Task<TurmaHorario> SeedScheduleAsync(
        ApplicationDbContext dbContext,
        DayOfWeek dayOfWeek,
        bool active = true)
    {
        var classroom = new Turma { Nome = "Turma recorrente", Modalidade = "Taiko", Ativa = true };
        var schedule = new TurmaHorario
        {
            Turma = classroom,
            DiaSemana = dayOfWeek,
            HoraInicio = new TimeOnly(19, 0),
            HoraFim = new TimeOnly(20, 30),
            Ativo = active
        };
        dbContext.AddRange(classroom, schedule);
        await dbContext.SaveChangesAsync();
        return schedule;
    }

    private static UsuarioSistema CreateInstructor(string login)
    {
        return new UsuarioSistema
        {
            Login = login,
            LoginNormalizado = login.ToUpperInvariant(),
            NomeExibicao = login,
            SenhaHash = "hash",
            TipoAcesso = TipoAcessoEnum.Funcionario,
            Ativo = true
        };
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
