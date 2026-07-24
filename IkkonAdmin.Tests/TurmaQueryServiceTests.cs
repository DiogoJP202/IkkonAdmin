using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class TurmaQueryServiceTests
{
    [Fact]
    public async Task ListarAsync_AplicaBuscaStatusEContaVinculos()
    {
        await using var dbContext = CriarDbContext();
        var baseTaiko = CriarTurma("Taiko Base", "Taiko", "Seg e Qua - 19h30", ativa: true);
        var avancado = CriarTurma("Taiko Avancado", "Taiko", "Sabado - 10h", ativa: false);
        var shinobue = CriarTurma("Shinobue Base", "Shinobue", "Terca - 20h", ativa: true);
        var aluno = CriarAluno("Kenji Mori", StatusAlunoEnum.Ativo);

        dbContext.AddRange(baseTaiko, avancado, shinobue, aluno);
        await dbContext.SaveChangesAsync();

        dbContext.AlunosTurmas.Add(new AlunoTurma
        {
            AlunoId = aluno.Id,
            TurmaId = baseTaiko.Id,
            DataVinculo = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = new TurmaQueryService(dbContext);

        var turmas = await service.ListarAsync("Taiko", ativa: true);

        var turma = Assert.Single(turmas);
        Assert.Equal(baseTaiko.Id, turma.Id);
        Assert.Single(turma.AlunoTurmas);
    }

    [Fact]
    public async Task ObterComAlunosAsync_CarregaAlunosVinculados()
    {
        await using var dbContext = CriarDbContext();
        var turma = CriarTurma("Taiko Intermediario", "Taiko", "Sexta - 19h", ativa: true);
        var aluno = CriarAluno("Marina Tanaka", StatusAlunoEnum.Ativo);

        dbContext.AddRange(turma, aluno);
        await dbContext.SaveChangesAsync();

        dbContext.AlunosTurmas.Add(new AlunoTurma
        {
            AlunoId = aluno.Id,
            TurmaId = turma.Id,
            DataVinculo = new DateTime(2026, 7, 2, 12, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = new TurmaQueryService(dbContext);

        var detalhes = await service.ObterComAlunosAsync(turma.Id);

        Assert.NotNull(detalhes);
        var vinculo = Assert.Single(detalhes.AlunoTurmas);
        Assert.Equal("Marina Tanaka", vinculo.Aluno?.NomeCompleto);
    }

    [Fact]
    public async Task ListarAlunosVinculaveisAsync_IncluiDesligadoSomenteQuandoJaEstaNaTurmaAtual()
    {
        await using var dbContext = CriarDbContext();
        var turmaAtual = CriarTurma("Taiko Base", "Taiko", "Segunda", ativa: true);
        var outraTurma = CriarTurma("Taiko Avancado", "Taiko", "Sabado", ativa: true);
        var alunoAtivo = CriarAluno("Ana Mori", StatusAlunoEnum.Ativo);
        var desligadoNaTurmaAtual = CriarAluno("Bruno Dias", StatusAlunoEnum.Desligado);
        var desligadoEmOutraTurma = CriarAluno("Rafael Sato", StatusAlunoEnum.Desligado);

        dbContext.AddRange(turmaAtual, outraTurma, alunoAtivo, desligadoNaTurmaAtual, desligadoEmOutraTurma);
        await dbContext.SaveChangesAsync();

        dbContext.AlunosTurmas.AddRange(
            new AlunoTurma
            {
                AlunoId = desligadoNaTurmaAtual.Id,
                TurmaId = turmaAtual.Id,
                DataVinculo = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)
            },
            new AlunoTurma
            {
                AlunoId = desligadoEmOutraTurma.Id,
                TurmaId = outraTurma.Id,
                DataVinculo = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)
            });
        await dbContext.SaveChangesAsync();

        var service = new TurmaQueryService(dbContext);

        var alunos = await service.ListarAlunosVinculaveisAsync(turmaAtual.Id);

        Assert.Contains(alunos, x => x.Id == alunoAtivo.Id);
        Assert.Contains(alunos, x => x.Id == desligadoNaTurmaAtual.Id);
        Assert.DoesNotContain(alunos, x => x.Id == desligadoEmOutraTurma.Id);
    }

    [Fact]
    public async Task ExisteNomeAsync_NormalizaEntradaEIgnoraTurmaAtual()
    {
        await using var dbContext = CriarDbContext();
        var turma = CriarTurma("Taiko Base", "Taiko", "Segunda", ativa: true);
        dbContext.Turmas.Add(turma);
        await dbContext.SaveChangesAsync();

        var service = new TurmaQueryService(dbContext);

        Assert.True(await service.ExisteNomeAsync("  Taiko Base  "));
        Assert.False(await service.ExisteNomeAsync("Taiko Base", turma.Id));
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Turma CriarTurma(string nome, string modalidade, string horario, bool ativa)
    {
        return new Turma
        {
            Nome = nome,
            Modalidade = modalidade,
            Horario = horario,
            Ativa = ativa
        };
    }

    private static Aluno CriarAluno(string nome, StatusAlunoEnum status)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = Guid.NewGuid().ToString("N")[..11],
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = status
        };
    }
}
