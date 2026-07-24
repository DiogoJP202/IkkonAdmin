using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class FinanceiroQueryServiceTests
{
    [Fact]
    public async Task ObterResumoAsync_AplicaBuscaStatusEPreservaIndicadoresGlobais()
    {
        await using var dbContext = CriarDbContext();
        var ana = CriarAluno("Ana Mori", "111.111.111-11");
        var bruno = CriarAluno("Bruno Sato", "222.222.222-22");

        dbContext.AddRange(ana, bruno);
        await dbContext.SaveChangesAsync();

        var mensalidadeAna = CriarMensalidade(ana.Id, StatusMensalidadeEnum.Pago, 260m, 260m, new DateOnly(2026, 7, 10));
        var mensalidadeBruno = CriarMensalidade(bruno.Id, StatusMensalidadeEnum.Atrasado, 260m, 220m, new DateOnly(2026, 6, 10));
        dbContext.Mensalidades.AddRange(mensalidadeAna, mensalidadeBruno);
        await dbContext.SaveChangesAsync();

        dbContext.Pagamentos.Add(new Pagamento
        {
            AlunoId = ana.Id,
            MensalidadeId = mensalidadeAna.Id,
            DataPagamento = new DateTime(2026, 7, 4, 18, 0, 0),
            ValorPago = 260m,
            FormaPagamento = FormaPagamentoEnum.Pix
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ObterResumoAsync(
            buscaAluno: "Ana",
            statusFiltro: StatusMensalidadeEnum.Pago,
            pagina: 1,
            tamanhoPagina: 20);

        var item = Assert.Single(resultado.Mensalidades);
        Assert.Equal(ana.Id, item.AlunoId);
        Assert.Equal(1, resultado.TotalRegistros);
        Assert.Equal(0, resultado.Pendentes);
        Assert.Equal(1, resultado.Atrasadas);
        Assert.Equal(260m, resultado.ValorRecebidoMes);
        Assert.Equal(220m, resultado.ValorEmAberto);
        Assert.Equal(7, resultado.MesCompetenciaGeracao);
        Assert.Equal(2026, resultado.AnoCompetenciaGeracao);
    }

    [Fact]
    public async Task ObterHistoricoAlunoAsync_CalculaPagamentosEAbertosDoAluno()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Base", Modalidade = "Taiko", Ativa = true };
        var aluno = CriarAluno("Marina Tanaka", "333.333.333-33");
        aluno.Turma = turma;

        dbContext.AddRange(turma, aluno);
        await dbContext.SaveChangesAsync();

        var mensalidadePaga = CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pago, 260m, 240m, new DateOnly(2026, 7, 10));
        var mensalidadeAberta = CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pendente, 260m, 260m, new DateOnly(2026, 8, 10));
        dbContext.Mensalidades.AddRange(mensalidadePaga, mensalidadeAberta);
        await dbContext.SaveChangesAsync();

        dbContext.Pagamentos.Add(new Pagamento
        {
            AlunoId = aluno.Id,
            MensalidadeId = mensalidadePaga.Id,
            DataPagamento = new DateTime(2026, 7, 8, 19, 0, 0),
            ValorPago = 240m,
            FormaPagamento = FormaPagamentoEnum.Dinheiro,
            Observacoes = "Desconto manual"
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var historico = await service.ObterHistoricoAlunoAsync(aluno.Id);

        Assert.NotNull(historico);
        Assert.Equal(aluno.Id, historico.AlunoId);
        Assert.Equal("Taiko Base", historico.Turma);
        Assert.Equal(240m, historico.TotalPago);
        Assert.Equal(260m, historico.TotalEmAberto);
        Assert.Equal(2, historico.Mensalidades.Count);
        Assert.Single(historico.Pagamentos);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static FinanceiroQueryService CriarService(ApplicationDbContext dbContext)
    {
        return new FinanceiroQueryService(dbContext, new TestClock());
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

    private static Mensalidade CriarMensalidade(
        int alunoId,
        StatusMensalidadeEnum status,
        decimal valorBase,
        decimal valorFinal,
        DateOnly dataVencimento)
    {
        return new Mensalidade
        {
            AlunoId = alunoId,
            Competencia = new DateOnly(dataVencimento.Year, dataVencimento.Month, 1),
            DataVencimento = dataVencimento,
            ValorBase = valorBase,
            ValorFinal = valorFinal,
            Status = status,
            DataPagamento = status == StatusMensalidadeEnum.Pago ? dataVencimento : null
        };
    }

    private sealed class TestClock : IClock
    {
        private static readonly DateTime FixedUtcNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedUtcNow.ToLocalTime();
        public DateTime Today => FixedUtcNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
