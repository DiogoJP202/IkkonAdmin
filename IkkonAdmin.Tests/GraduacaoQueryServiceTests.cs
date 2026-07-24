using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class GraduacaoQueryServiceTests
{
    [Fact]
    public async Task ListarAsync_AplicaBuscaAprovacaoECarregaRelacionamentos()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Intermediaria", Modalidade = "Taiko", Ativa = true };
        var marina = CriarAluno("Marina Tanaka", "111.222.333-44", turma, StatusAlunoEnum.Ativo);
        var kenji = CriarAluno("Kenji Mori", "222.333.444-55", turma, StatusAlunoEnum.Ativo);
        var exame = new ExameGraduacao
        {
            DataExame = new DateOnly(2026, 7, 10),
            Local = "Dojo",
            NivelPretendido = NivelGraduacaoEnum.Intermediario
        };

        dbContext.AddRange(turma, marina, kenji, exame);
        await dbContext.SaveChangesAsync();

        dbContext.Graduacoes.AddRange(
            CriarGraduacao(marina.Id, exame.Id, new DateOnly(2026, 7, 12), true, NivelGraduacaoEnum.Basico, NivelGraduacaoEnum.Intermediario),
            CriarGraduacao(kenji.Id, exame.Id, new DateOnly(2026, 7, 13), false, NivelGraduacaoEnum.Basico, null));
        await dbContext.SaveChangesAsync();

        var service = new GraduacaoQueryService(dbContext);

        var graduacoes = await service.ListarAsync("Marina", somenteAprovados: true);

        var graduacao = Assert.Single(graduacoes);
        Assert.Equal(marina.Id, graduacao.AlunoId);
        Assert.Equal("Marina Tanaka", graduacao.Aluno?.NomeCompleto);
        Assert.Equal("Taiko Intermediaria", graduacao.Aluno?.Turma?.Nome);
        Assert.Equal(new DateOnly(2026, 7, 10), graduacao.ExameGraduacao?.DataExame);
    }

    [Fact]
    public async Task ObterNivelAtualAsync_UsaUltimaGraduacaoAprovadaOuIniciante()
    {
        await using var dbContext = CriarDbContext();
        var alunoComHistorico = CriarAluno("Rafael Sato", "333.444.555-66", null, StatusAlunoEnum.Ativo);
        var alunoSemHistorico = CriarAluno("Ana Mori", "444.555.666-77", null, StatusAlunoEnum.Ativo);

        dbContext.Alunos.AddRange(alunoComHistorico, alunoSemHistorico);
        await dbContext.SaveChangesAsync();

        dbContext.Graduacoes.AddRange(
            CriarGraduacao(alunoComHistorico.Id, null, new DateOnly(2026, 5, 10), true, NivelGraduacaoEnum.Iniciante, NivelGraduacaoEnum.Basico),
            CriarGraduacao(alunoComHistorico.Id, null, new DateOnly(2026, 6, 10), false, NivelGraduacaoEnum.Basico, null),
            CriarGraduacao(alunoComHistorico.Id, null, new DateOnly(2026, 7, 10), true, NivelGraduacaoEnum.Basico, NivelGraduacaoEnum.Intermediario));
        await dbContext.SaveChangesAsync();

        var service = new GraduacaoQueryService(dbContext);

        var nivelAtual = await service.ObterNivelAtualAsync(alunoComHistorico.Id);
        var nivelInicial = await service.ObterNivelAtualAsync(alunoSemHistorico.Id);

        Assert.Equal(NivelGraduacaoEnum.Intermediario, nivelAtual);
        Assert.Equal(NivelGraduacaoEnum.Iniciante, nivelInicial);
    }

    [Fact]
    public async Task ListarAlunosAptosAsync_RetornaApenasAtivosOrdenados()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Base", Modalidade = "Taiko", Ativa = true };

        dbContext.AddRange(
            turma,
            CriarAluno("Bruno Dias", "555.666.777-88", turma, StatusAlunoEnum.Desligado),
            CriarAluno("Ana Mori", "666.777.888-99", turma, StatusAlunoEnum.Ativo),
            CriarAluno("Kenji Mori", "777.888.999-00", turma, StatusAlunoEnum.Ativo));
        await dbContext.SaveChangesAsync();

        var service = new GraduacaoQueryService(dbContext);

        var alunos = await service.ListarAlunosAptosAsync();

        Assert.Collection(
            alunos,
            aluno => Assert.Equal("Ana Mori", aluno.NomeCompleto),
            aluno => Assert.Equal("Kenji Mori", aluno.NomeCompleto));
        Assert.All(alunos, aluno => Assert.Equal(StatusAlunoEnum.Ativo, aluno.Status));
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Aluno CriarAluno(string nome, string cpf, Turma? turma, StatusAlunoEnum status)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = cpf,
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = status,
            Turma = turma
        };
    }

    private static Graduacao CriarGraduacao(
        int alunoId,
        int? exameId,
        DateOnly dataResultado,
        bool aprovado,
        NivelGraduacaoEnum nivelAnterior,
        NivelGraduacaoEnum? nivelNovo)
    {
        return new Graduacao
        {
            AlunoId = alunoId,
            ExameGraduacaoId = exameId,
            DataResultado = dataResultado,
            ResultadoAprovado = aprovado,
            NivelAnterior = nivelAnterior,
            NivelNovo = nivelNovo
        };
    }
}
