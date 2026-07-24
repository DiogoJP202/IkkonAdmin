using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class AdmissaoQueryServiceTests
{
    [Fact]
    public async Task ListarAsync_AplicaBuscaStatusEOrdenaPorAulaExperimental()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Marina Tanaka", "11122233344");

        dbContext.Add(aluno);
        await dbContext.SaveChangesAsync();

        dbContext.Admissoes.AddRange(
            CriarAdmissao("Interessado antigo", new DateOnly(2026, 7, 1), StatusAdmissaoEnum.EmAndamento),
            CriarAdmissao("Visitante cancelado", new DateOnly(2026, 7, 20), StatusAdmissaoEnum.Cancelado),
            new Admissao
            {
                NomeInteressado = "Responsavel da Marina",
                AlunoId = aluno.Id,
                DataAulaExperimental = new DateOnly(2026, 7, 15),
                Status = StatusAdmissaoEnum.EmAndamento
            });
        await dbContext.SaveChangesAsync();

        var service = new AdmissaoQueryService(dbContext);

        var admissoes = await service.ListarAsync("Marina", StatusAdmissaoEnum.EmAndamento);

        var admissao = Assert.Single(admissoes);
        Assert.Equal("Responsavel da Marina", admissao.NomeInteressado);
        Assert.Equal("Marina Tanaka", admissao.Aluno?.NomeCompleto);
    }

    [Fact]
    public async Task ObterDetalhesAsync_CarregaAlunoVinculado()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Kenji Mori", "22233344455");
        var admissao = CriarAdmissao("Kenji Mori", new DateOnly(2026, 7, 10), StatusAdmissaoEnum.Matriculado);
        admissao.Aluno = aluno;

        dbContext.AddRange(aluno, admissao);
        await dbContext.SaveChangesAsync();

        var service = new AdmissaoQueryService(dbContext);

        var detalhes = await service.ObterDetalhesAsync(admissao.Id);

        Assert.NotNull(detalhes);
        Assert.Equal("Kenji Mori", detalhes.Aluno?.NomeCompleto);
    }

    [Fact]
    public async Task ListarTurmasAsync_RetornaSomenteAtivasOrdenadas()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Turmas.AddRange(
            new Turma { Nome = "Z Taiko", Modalidade = "Taiko", Ativa = true },
            new Turma { Nome = "A Shinobue", Modalidade = "Shinobue", Ativa = true },
            new Turma { Nome = "Inativa", Modalidade = "Taiko", Ativa = false });
        await dbContext.SaveChangesAsync();

        var service = new AdmissaoQueryService(dbContext);

        var turmas = await service.ListarTurmasAsync();

        Assert.Collection(
            turmas,
            turma => Assert.Equal("A Shinobue", turma.Nome),
            turma => Assert.Equal("Z Taiko", turma.Nome));
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Admissao CriarAdmissao(
        string nome,
        DateOnly dataAulaExperimental,
        StatusAdmissaoEnum status)
    {
        return new Admissao
        {
            NomeInteressado = nome,
            DataAulaExperimental = dataAulaExperimental,
            Status = status
        };
    }

    private static Aluno CriarAluno(string nome, string cpf)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = cpf,
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo
        };
    }
}
