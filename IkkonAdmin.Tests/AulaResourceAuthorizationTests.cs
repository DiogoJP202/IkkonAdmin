using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class AulaResourceAuthorizationTests
{
    [Fact]
    public async Task Instrutor_ListaSomenteAulaExplicitamenteAtribuidaAEle()
    {
        await using var dbContext = CreateDbContext();
        var (ownInstructor, otherInstructor, classroom) = await SeedBaseAsync(dbContext);
        var ownLesson = CreateLesson(classroom.Id, ownInstructor.Id, new DateTime(2026, 7, 13, 19, 0, 0));
        var otherLesson = CreateLesson(classroom.Id, otherInstructor.Id, new DateTime(2026, 7, 14, 19, 0, 0));
        dbContext.Aulas.AddRange(ownLesson, otherLesson);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var result = await service.ObterFrequenciaAsync(
            new FrequenciaAdminFilter(),
            AulaAccessScope.Restricted(ownInstructor.Id));

        Assert.Single(result.Aulas);
        Assert.Equal(ownLesson.Id, result.Aulas.Single().Id);
    }

    [Fact]
    public async Task Instrutor_NaoAbreNemAlteraAulaForaDoEscopo()
    {
        await using var dbContext = CreateDbContext();
        var (instructor, otherInstructor, classroom) = await SeedBaseAsync(dbContext);
        var lesson = CreateLesson(classroom.Id, otherInstructor.Id, new DateTime(2026, 7, 13, 19, 0, 0));
        dbContext.Aulas.Add(lesson);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var scope = AulaAccessScope.Restricted(instructor.Id);

        var details = await service.ObterRegistroFrequenciaAsync(lesson.Id, scope);
        var save = await service.SalvarFrequenciaAsync(
            new FrequenciaRegistroPostViewModel { AulaId = lesson.Id },
            scope);

        Assert.Null(details);
        Assert.False(save.Success);
        Assert.Equal(OperationResultStatus.NotFound, save.Status);
        Assert.Equal(StatusAulaEnum.Agendada, (await dbContext.Aulas.FindAsync(lesson.Id))!.Status);
    }

    [Fact]
    public async Task AulaSemInstrutor_AceitaVinculoAtivoNaTurmaNaData()
    {
        await using var dbContext = CreateDbContext();
        var (instructor, _, classroom) = await SeedBaseAsync(dbContext);
        var lesson = CreateLesson(classroom.Id, null, new DateTime(2026, 7, 13, 19, 0, 0));
        dbContext.Aulas.Add(lesson);
        dbContext.TurmaInstrutores.Add(new TurmaInstrutor
        {
            TurmaId = classroom.Id,
            UsuarioSistemaId = instructor.Id,
            DataInicio = new DateOnly(2026, 7, 1),
            DataFim = new DateOnly(2026, 7, 31)
        });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var details = await service.ObterRegistroFrequenciaAsync(
            lesson.Id,
            AulaAccessScope.Restricted(instructor.Id));

        Assert.NotNull(details);
        Assert.Equal(lesson.Id, details.AulaId);
    }

    [Fact]
    public async Task AulaSemInstrutor_RejeitaVinculoEncerradoAntesDaData()
    {
        await using var dbContext = CreateDbContext();
        var (instructor, _, classroom) = await SeedBaseAsync(dbContext);
        var lesson = CreateLesson(classroom.Id, null, new DateTime(2026, 7, 13, 19, 0, 0));
        dbContext.Aulas.Add(lesson);
        dbContext.TurmaInstrutores.Add(new TurmaInstrutor
        {
            TurmaId = classroom.Id,
            UsuarioSistemaId = instructor.Id,
            DataInicio = new DateOnly(2026, 6, 1),
            DataFim = new DateOnly(2026, 6, 30)
        });
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var details = await service.ObterRegistroFrequenciaAsync(
            lesson.Id,
            AulaAccessScope.Restricted(instructor.Id));

        Assert.Null(details);
    }

    [Fact]
    public async Task Admin_ComEscopoGlobal_AcessaQualquerAula()
    {
        await using var dbContext = CreateDbContext();
        var (admin, otherInstructor, classroom) = await SeedBaseAsync(dbContext);
        var lesson = CreateLesson(classroom.Id, otherInstructor.Id, new DateTime(2026, 7, 13, 19, 0, 0));
        dbContext.Aulas.Add(lesson);
        await dbContext.SaveChangesAsync();

        var service = CreateService(dbContext);
        var details = await service.ObterRegistroFrequenciaAsync(
            lesson.Id,
            AulaAccessScope.Global(admin.Id));

        Assert.NotNull(details);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static AreaAlunoAulasAdminService CreateService(ApplicationDbContext dbContext)
    {
        return new AreaAlunoAulasAdminService(
            dbContext,
            new TestClock(),
            new RecordingAuditLogger(),
            new StubCurrentUserService(),
            new InsigniaRuleEvaluator(dbContext, new TestClock()));
    }

    private static async Task<(UsuarioSistema First, UsuarioSistema Second, Turma Classroom)> SeedBaseAsync(
        ApplicationDbContext dbContext)
    {
        var first = CreateUser("instrutor.um");
        var second = CreateUser("instrutor.dois");
        var classroom = new Turma { Nome = "Taiko", Modalidade = "Taiko", Ativa = true };
        dbContext.AddRange(first, second, classroom);
        await dbContext.SaveChangesAsync();
        return (first, second, classroom);
    }

    private static UsuarioSistema CreateUser(string login)
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

    private static Aula CreateLesson(int classroomId, int? instructorId, DateTime start)
    {
        return new Aula
        {
            TurmaId = classroomId,
            InstrutorUsuarioId = instructorId,
            Inicio = start,
            Fim = start.AddHours(1),
            Status = StatusAulaEnum.Agendada
        };
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow => new(2026, 7, 13, 15, 0, 0, DateTimeKind.Utc);
        public DateTime Now => new(2026, 7, 13, 12, 0, 0);
        public DateTime Today => Now.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
