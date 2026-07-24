using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class TurmaServiceTests
{
    [Fact]
    public async Task CriarAsync_VinculaAlunosSelecionadosEPreencheTurmaPrincipalQuandoVazia()
    {
        await using var dbContext = CriarDbContext();
        var alunoSemTurma = CriarAluno("Ana Mori", null);
        var turmaExistente = CriarTurma("Taiko Base");
        var alunoComTurma = CriarAluno("Kenji Mori", turmaExistente);

        dbContext.AddRange(turmaExistente, alunoSemTurma, alunoComTurma);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);
        var novaTurma = CriarTurma("Shinobue Base");

        var result = await service.CriarAsync(novaTurma, [alunoSemTurma.Id, alunoComTurma.Id, alunoComTurma.Id]);
        var turmaId = Assert.IsType<int>(result.Value);

        var vinculos = await dbContext.AlunosTurmas
            .Where(x => x.TurmaId == turmaId)
            .OrderBy(x => x.AlunoId)
            .ToListAsync();
        var alunoSemTurmaAtualizado = await dbContext.Alunos.FindAsync(alunoSemTurma.Id);
        var alunoComTurmaAtualizado = await dbContext.Alunos.FindAsync(alunoComTurma.Id);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal(2, vinculos.Count);
        Assert.All(vinculos, x => Assert.Equal(TestClock.FixedUtcNow, x.DataVinculo));
        Assert.Equal(turmaId, alunoSemTurmaAtualizado?.TurmaId);
        Assert.Equal(turmaExistente.Id, alunoComTurmaAtualizado?.TurmaId);
    }

    [Fact]
    public async Task CriarAsync_RetornaErroQuandoNomeDuplicado()
    {
        await using var dbContext = CriarDbContext();
        var turmaExistente = CriarTurma("Taiko Base");

        dbContext.Turmas.Add(turmaExistente);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);
        var result = await service.CriarAsync(CriarTurma(" Taiko Base "), []);

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(Turma.Nome), erro.Field);
        Assert.Equal("Já existe uma turma com esse nome.", erro.Message);
        Assert.Equal(1, await dbContext.Turmas.CountAsync());
    }

    [Fact]
    public async Task AtualizarAsync_ReconciliaVinculosEAtualizaTurmaPrincipalLegada()
    {
        await using var dbContext = CriarDbContext();
        var turmaOriginal = CriarTurma("Taiko Base");
        var turmaSecundaria = CriarTurma("Shinobue Base");
        var alunoRemovido = CriarAluno("Bruno Dias", turmaOriginal);
        var alunoMantido = CriarAluno("Marina Tanaka", turmaOriginal);
        var alunoAdicionado = CriarAluno("Rafael Sato", null);

        dbContext.AddRange(turmaOriginal, turmaSecundaria, alunoRemovido, alunoMantido, alunoAdicionado);
        await dbContext.SaveChangesAsync();

        dbContext.AlunosTurmas.AddRange(
            CriarVinculo(alunoRemovido.Id, turmaOriginal.Id),
            CriarVinculo(alunoRemovido.Id, turmaSecundaria.Id),
            CriarVinculo(alunoMantido.Id, turmaOriginal.Id));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var atualizada = await service.AtualizarAsync(
            turmaOriginal.Id,
            new Turma
            {
                Nome = "Taiko Base Atualizada",
                Modalidade = "Taiko",
                Horario = "Quarta",
                Ativa = false,
                Observacoes = "Nova observacao"
            },
            [alunoMantido.Id, alunoAdicionado.Id]);

        var turma = await dbContext.Turmas
            .Include(x => x.AlunoTurmas)
            .FirstAsync(x => x.Id == turmaOriginal.Id);
        var removido = await dbContext.Alunos.FindAsync(alunoRemovido.Id);
        var adicionado = await dbContext.Alunos
            .Include(x => x.AlunoTurmas)
            .FirstAsync(x => x.Id == alunoAdicionado.Id);

        Assert.True(atualizada.Success);
        Assert.Equal(OperationResultStatus.Success, atualizada.Status);
        Assert.Equal("Taiko Base Atualizada", turma.Nome);
        Assert.False(turma.Ativa);
        Assert.Equal(turmaSecundaria.Id, removido?.TurmaId);
        Assert.Contains(turma.AlunoTurmas, x => x.AlunoId == alunoMantido.Id);
        Assert.Contains(turma.AlunoTurmas, x => x.AlunoId == alunoAdicionado.Id);
        Assert.DoesNotContain(turma.AlunoTurmas, x => x.AlunoId == alunoRemovido.Id);
        Assert.Equal(turmaOriginal.Id, adicionado.TurmaId);
        Assert.Contains(adicionado.AlunoTurmas, x =>
            x.TurmaId == turmaOriginal.Id &&
            x.DataVinculo == TestClock.FixedUtcNow);
    }

    [Fact]
    public async Task AtualizarAsync_RetornaErroQuandoNomeDuplicado()
    {
        await using var dbContext = CriarDbContext();
        var turmaExistente = CriarTurma("Taiko Base");
        var turmaAtual = CriarTurma("Shinobue Base");

        dbContext.AddRange(turmaExistente, turmaAtual);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);
        var result = await service.AtualizarAsync(
            turmaAtual.Id,
            new Turma
            {
                Nome = " Taiko Base ",
                Modalidade = "Taiko",
                Horario = "Quarta",
                Ativa = true
            },
            []);

        var erro = Assert.Single(result.Errors);
        var turmaSemAlteracao = await dbContext.Turmas.FindAsync(turmaAtual.Id);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(Turma.Nome), erro.Field);
        Assert.Equal("Já existe uma turma com esse nome.", erro.Message);
        Assert.Equal("Shinobue Base", turmaSemAlteracao?.Nome);
    }

    [Fact]
    public async Task AtualizarAsync_RetornaNaoEncontradoQuandoTurmaNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AtualizarAsync(
            123,
            new Turma
            {
                Nome = "Taiko Base",
                Modalidade = "Taiko",
                Horario = "Quarta",
                Ativa = true
            },
            []);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("Turma não encontrada.", result.Message);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static TurmaService CriarService(ApplicationDbContext dbContext)
    {
        return new TurmaService(dbContext, new TestClock(), new TurmaQueryService(dbContext));
    }

    private static Turma CriarTurma(string nome)
    {
        return new Turma
        {
            Nome = nome,
            Modalidade = "Taiko",
            Horario = "Segunda",
            Ativa = true
        };
    }

    private static Aluno CriarAluno(string nome, Turma? turma)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = Guid.NewGuid().ToString("N")[..11],
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo,
            Turma = turma
        };
    }

    private static AlunoTurma CriarVinculo(int alunoId, int turmaId)
    {
        return new AlunoTurma
        {
            AlunoId = alunoId,
            TurmaId = turmaId,
            DataVinculo = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)
        };
    }

    private sealed class TestClock : IClock
    {
        public static readonly DateTime FixedUtcNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedUtcNow.ToLocalTime();
        public DateTime Today => FixedUtcNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
