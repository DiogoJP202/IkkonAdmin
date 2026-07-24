using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class AlunoQueryServiceTests
{
    [Fact]
    public async Task ListarAsync_AplicaBuscaStatusETurmaSecundaria()
    {
        await using var dbContext = CriarDbContext();
        var turmaBase = new Turma { Nome = "Shinobue Base", Modalidade = "Taiko", Ativa = true };
        var turmaIntermediaria = new Turma { Nome = "Taiko Intermediaria", Modalidade = "Taiko", Ativa = true };
        var ana = CriarAluno("Ana Mori", "111.222.333-44", "(11) 99999-0001", turmaBase, StatusAlunoEnum.Ativo);
        var bruno = CriarAluno("Bruno Sato", "222.333.444-55", "(11) 99999-0002", turmaIntermediaria, StatusAlunoEnum.Ativo);
        var carla = CriarAluno("Carla Tanaka", "333.444.555-66", "(11) 99999-0002", turmaBase, StatusAlunoEnum.Inativo);

        dbContext.AddRange(turmaBase, turmaIntermediaria, ana, bruno, carla);
        await dbContext.SaveChangesAsync();

        dbContext.AlunosTurmas.Add(new AlunoTurma
        {
            AlunoId = bruno.Id,
            TurmaId = turmaBase.Id,
            DataVinculo = new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = new AlunoQueryService(dbContext);

        var resultado = await service.ListarAsync(
            busca: "999990002",
            status: StatusAlunoEnum.Ativo,
            turmaId: turmaBase.Id,
            pagina: 1,
            tamanhoPagina: 20);

        var aluno = Assert.Single(resultado.Itens);
        Assert.Equal(bruno.Id, aluno.Id);
        Assert.Equal(1, resultado.TotalRegistros);
        Assert.Equal("Taiko Intermediaria", aluno.Turma?.Nome);
    }

    [Fact]
    public async Task ObterDetalhesAsync_CarregaRelacionamentosFinanceirosEHistorico()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Base", Modalidade = "Taiko", Ativa = true };
        var aluno = CriarAluno("Marina Tanaka", "444.555.666-77", "(11) 99999-0003", turma, StatusAlunoEnum.Ativo);

        dbContext.AddRange(turma, aluno);
        await dbContext.SaveChangesAsync();

        var mensalidade = new Mensalidade
        {
            AlunoId = aluno.Id,
            Competencia = new DateOnly(2026, 7, 1),
            DataVencimento = new DateOnly(2026, 7, 10),
            ValorBase = 260m,
            ValorFinal = 240m,
            Status = StatusMensalidadeEnum.Pago,
            DataPagamento = new DateOnly(2026, 7, 8)
        };

        dbContext.Mensalidades.Add(mensalidade);
        await dbContext.SaveChangesAsync();

        dbContext.Pagamentos.Add(new Pagamento
        {
            AlunoId = aluno.Id,
            MensalidadeId = mensalidade.Id,
            DataPagamento = new DateTime(2026, 7, 8, 19, 0, 0),
            ValorPago = 240m,
            FormaPagamento = FormaPagamentoEnum.Pix
        });
        dbContext.HistoricosAlunos.Add(new HistoricoAluno
        {
            AlunoId = aluno.Id,
            DataEvento = new DateTime(2026, 7, 9, 10, 0, 0),
            TipoEvento = "Financeiro",
            Descricao = "Pagamento registrado"
        });
        await dbContext.SaveChangesAsync();

        var service = new AlunoQueryService(dbContext);

        var detalhes = await service.ObterDetalhesAsync(aluno.Id);

        Assert.NotNull(detalhes);
        Assert.Equal("Taiko Base", detalhes.Turma?.Nome);
        Assert.Single(detalhes.Mensalidades);
        Assert.Single(detalhes.Pagamentos);
        Assert.Single(detalhes.Historicos);
    }

    [Fact]
    public async Task ExisteCpfAsync_NormalizaCpfEPermiteIgnorarAlunoAtual()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Kenji Mori", "555.666.777-88", null, null, StatusAlunoEnum.Ativo);
        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        var service = new AlunoQueryService(dbContext);

        Assert.True(await service.ExisteCpfAsync("55566677788"));
        Assert.False(await service.ExisteCpfAsync("55566677788", aluno.Id));
        Assert.False(await service.ExisteCpfAsync(""));
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Aluno CriarAluno(
        string nome,
        string cpf,
        string? celular,
        Turma? turma,
        StatusAlunoEnum status)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = cpf,
            Celular = celular,
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = status,
            Turma = turma
        };
    }
}
