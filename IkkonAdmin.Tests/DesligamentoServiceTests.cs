using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IkkonAdmin.Tests;

public class DesligamentoServiceTests
{
    [Fact]
    public async Task CriarAsync_TrimmaCamposEBloqueiaProcessoAbertoDuplicado()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Ana Mori");
        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.CriarAsync(new Desligamento
        {
            AlunoId = aluno.Id,
            DataSolicitacao = new DateOnly(2026, 7, 10),
            Motivo = "  Mudanca de cidade  ",
            Observacoes = "  Entrar em contato no fim do mes  "
        });

        var salvo = await dbContext.Desligamentos.FindAsync(result.Value);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Mudanca de cidade", salvo?.Motivo);
        Assert.Equal("Entrar em contato no fim do mes", salvo?.Observacoes);
        var duplicado = await service.CriarAsync(new Desligamento
        {
            AlunoId = aluno.Id,
            DataSolicitacao = new DateOnly(2026, 7, 11),
            Motivo = "Duplicado"
        });

        Assert.False(duplicado.Success);
        Assert.Equal(OperationResultStatus.ValidationError, duplicado.Status);
        Assert.Equal("Já existe um processo de desligamento em aberto para este aluno.", duplicado.Message);
    }

    [Fact]
    public async Task AtualizarAsync_NormalizaCamposEAtualizaProcesso()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Rafael Sato");
        var desligamento = new Desligamento
        {
            Aluno = aluno,
            DataSolicitacao = new DateOnly(2026, 7, 10),
            Motivo = "Solicitacao inicial"
        };

        dbContext.Desligamentos.Add(desligamento);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.AtualizarAsync(
            desligamento.Id,
            "  Mudanca de cidade  ",
            pendenciaFinanceira: 220.456m,
            multaRescisoria: 50.555m,
            requerimentoRecebido: true,
            acessosRemovidos: true,
            observacoes: "  Retirar do grupo interno  ");

        var atualizado = await dbContext.Desligamentos.FindAsync(desligamento.Id);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Processo de desligamento atualizado.", result.Message);
        Assert.Equal("Mudanca de cidade", atualizado?.Motivo);
        Assert.Equal(220.46m, atualizado?.PendenciaFinanceira);
        Assert.Equal(50.56m, atualizado?.MultaRescisoria);
        Assert.True(atualizado?.RequerimentoRecebido);
        Assert.True(atualizado?.AcessosRemovidos);
        Assert.Equal("Retirar do grupo interno", atualizado?.Observacoes);
    }

    [Fact]
    public async Task AtualizarAsync_RetornaNaoEncontradoQuandoDesligamentoNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AtualizarAsync(
            123,
            "Motivo",
            pendenciaFinanceira: 0m,
            multaRescisoria: 0m,
            requerimentoRecebido: false,
            acessosRemovidos: false,
            observacoes: null);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("Desligamento não encontrado.", result.Message);
    }

    [Fact]
    public async Task ConfirmarAsync_AtualizaAlunoHistoricoECancelaCobrancasFuturas()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Kenji Mori");
        var desligamento = new Desligamento
        {
            Aluno = aluno,
            DataSolicitacao = new DateOnly(2026, 7, 10),
            Motivo = "Encerramento solicitado",
            PendenciaFinanceira = 220m,
            MultaRescisoria = 50m
        };

        dbContext.Desligamentos.Add(desligamento);
        await dbContext.SaveChangesAsync();

        var mensalidadeAtual = CriarMensalidade(aluno.Id, new DateOnly(2026, 7, 10), StatusMensalidadeEnum.Pendente);
        var mensalidadeFutura = CriarMensalidade(aluno.Id, new DateOnly(2026, 8, 10), StatusMensalidadeEnum.Pendente);
        mensalidadeFutura.Observacoes = "Acordo existente.";
        var mensalidadeFuturaPaga = CriarMensalidade(aluno.Id, new DateOnly(2026, 9, 10), StatusMensalidadeEnum.Pago);

        dbContext.Mensalidades.AddRange(mensalidadeAtual, mensalidadeFutura, mensalidadeFuturaPaga);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ConfirmarAsync(desligamento.Id, encerrarCobrancasFuturas: true);

        var alunoAtualizado = await dbContext.Alunos.FindAsync(aluno.Id);
        var desligamentoAtualizado = await dbContext.Desligamentos.FindAsync(desligamento.Id);
        var historico = await dbContext.HistoricosAlunos.SingleAsync(x => x.AlunoId == aluno.Id);
        var mensalidadeAtualizada = await dbContext.Mensalidades.FindAsync(mensalidadeFutura.Id);
        var mensalidadeAtualCompetencia = await dbContext.Mensalidades.FindAsync(mensalidadeAtual.Id);
        var mensalidadePaga = await dbContext.Mensalidades.FindAsync(mensalidadeFuturaPaga.Id);
        var confirmacao = Assert.IsType<DesligamentoConfirmacaoResultado>(resultado.Value);

        Assert.True(resultado.Success);
        Assert.Equal(OperationResultStatus.Success, resultado.Status);
        Assert.Equal("Desligamento confirmado.", resultado.Message);
        Assert.Equal(1, confirmacao.CobrancasCanceladas);
        Assert.Equal(aluno.Id, confirmacao.AlunoId);
        Assert.Equal(StatusAlunoEnum.Desligado, alunoAtualizado?.Status);
        Assert.Equal(new DateOnly(2026, 7, 13), desligamentoAtualizado?.DataConfirmacao);
        Assert.Equal(TestClock.FixedNow, historico.DataEvento);
        Assert.Equal("Desligamento", historico.TipoEvento);
        Assert.Equal(StatusMensalidadeEnum.Cancelado, mensalidadeAtualizada?.Status);
        Assert.Contains("Cancelada por desligamento do aluno.", mensalidadeAtualizada?.Observacoes);
        Assert.Equal(StatusMensalidadeEnum.Pendente, mensalidadeAtualCompetencia?.Status);
        Assert.Equal(StatusMensalidadeEnum.Pago, mensalidadePaga?.Status);
    }

    [Fact]
    public async Task ConfirmarAsync_NaoConfirmaDuasVezes()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Marina Tanaka");
        var desligamento = new Desligamento
        {
            Aluno = aluno,
            DataSolicitacao = new DateOnly(2026, 7, 10),
            DataConfirmacao = new DateOnly(2026, 7, 11),
            Motivo = "Ja confirmado"
        };

        dbContext.Desligamentos.Add(desligamento);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ConfirmarAsync(desligamento.Id, encerrarCobrancasFuturas: true);

        Assert.False(resultado.Success);
        Assert.Equal(OperationResultStatus.ValidationError, resultado.Status);
        Assert.Equal("Desligamento já confirmado.", resultado.Message);
        Assert.Null(resultado.Value);
        Assert.Empty(dbContext.HistoricosAlunos);
    }

    [Fact]
    public async Task ConfirmarAsync_RetornaNaoEncontradoQuandoDesligamentoNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var resultado = await service.ConfirmarAsync(123, encerrarCobrancasFuturas: true);

        Assert.False(resultado.Success);
        Assert.Equal(OperationResultStatus.NotFound, resultado.Status);
        Assert.Equal("Desligamento não encontrado.", resultado.Message);
        Assert.Null(resultado.Value);
        Assert.Empty(resultado.Errors);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static DesligamentoService CriarService(ApplicationDbContext dbContext)
    {
        return new DesligamentoService(
            dbContext,
            new TestClock());
    }

    private static Aluno CriarAluno(string nome)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = Guid.NewGuid().ToString("N")[..11],
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo
        };
    }

    private static Mensalidade CriarMensalidade(
        int alunoId,
        DateOnly vencimento,
        StatusMensalidadeEnum status)
    {
        return new Mensalidade
        {
            AlunoId = alunoId,
            Competencia = new DateOnly(vencimento.Year, vencimento.Month, 1),
            DataVencimento = vencimento,
            ValorBase = 260m,
            ValorFinal = 260m,
            Status = status
        };
    }

    private sealed class TestClock : IClock
    {
        public static readonly DateTime FixedUtcNow = new(2026, 7, 13, 15, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime FixedNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Local);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedNow;
        public DateTime Today => FixedNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
