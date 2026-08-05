using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class FinanceiroServiceTests
{
    [Fact]
    public async Task RegistrarPagamentoAsync_CriaPagamentoEAtualizaMensalidade()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var mensalidade = CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pendente);

        dbContext.Add(aluno);
        await dbContext.SaveChangesAsync();
        mensalidade.AlunoId = aluno.Id;
        dbContext.Mensalidades.Add(mensalidade);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.RegistrarPagamentoAsync(new RegistrarPagamentoViewModel
        {
            MensalidadeId = mensalidade.Id,
            AlunoId = aluno.Id,
            DataPagamento = new DateTime(2026, 7, 8, 19, 30, 0),
            ValorPago = 240m,
            FormaPagamento = FormaPagamentoEnum.Pix,
            Observacoes = " Pago no balcão "
        });

        var mensalidadeAtualizada = await dbContext.Mensalidades
            .Include(x => x.Pagamentos)
            .FirstAsync(x => x.Id == mensalidade.Id);
        var pagamento = Assert.Single(mensalidadeAtualizada.Pagamentos);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Pagamento registrado com sucesso.", result.Message);
        Assert.Equal(StatusMensalidadeEnum.Pago, mensalidadeAtualizada.Status);
        Assert.Equal(new DateOnly(2026, 7, 8), mensalidadeAtualizada.DataPagamento);
        Assert.Equal(240m, pagamento.ValorPago);
        Assert.Equal(FormaPagamentoEnum.Pix, pagamento.FormaPagamento);
        Assert.Equal("Pago no balcão", pagamento.Observacoes);
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_RetornaNaoEncontradoQuandoMensalidadeNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.RegistrarPagamentoAsync(new RegistrarPagamentoViewModel
        {
            MensalidadeId = 123,
            AlunoId = 456,
            DataPagamento = new DateTime(2026, 7, 8, 19, 30, 0),
            ValorPago = 240m,
            FormaPagamento = FormaPagamentoEnum.Pix
        });

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("Mensalidade não encontrada para registrar pagamento.", result.Message);
        Assert.Empty(dbContext.Pagamentos);
    }

    [Fact]
    public async Task RegistrarPagamentoAsync_RetornaErroQuandoMensalidadeNaoPertenceAoAluno()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var mensalidade = CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pendente);

        dbContext.Add(aluno);
        await dbContext.SaveChangesAsync();
        mensalidade.AlunoId = aluno.Id;
        dbContext.Mensalidades.Add(mensalidade);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.RegistrarPagamentoAsync(new RegistrarPagamentoViewModel
        {
            MensalidadeId = mensalidade.Id,
            AlunoId = aluno.Id + 1,
            DataPagamento = new DateTime(2026, 7, 8, 19, 30, 0),
            ValorPago = 240m,
            FormaPagamento = FormaPagamentoEnum.Pix
        });

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal("A mensalidade informada não pertence ao aluno selecionado.", erro.Message);
        Assert.Equal(StatusMensalidadeEnum.Pendente, mensalidade.Status);
        Assert.Empty(dbContext.Pagamentos);
    }

    [Fact]
    public async Task AtualizarValorFinalAsync_AtualizaValorArredondado()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var mensalidade = CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pendente);

        dbContext.Add(aluno);
        await dbContext.SaveChangesAsync();
        mensalidade.AlunoId = aluno.Id;
        dbContext.Mensalidades.Add(mensalidade);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.AtualizarValorFinalAsync(mensalidade.Id, 212.555m);

        var atualizada = await dbContext.Mensalidades.FindAsync(mensalidade.Id);
        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Valor final atualizado.", result.Message);
        Assert.Equal(212.56m, atualizada?.ValorFinal);
    }

    [Fact]
    public async Task AtualizarValorFinalAsync_RetornaErroQuandoValorNegativo()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AtualizarValorFinalAsync(1, -1m);

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(Mensalidade.ValorFinal), erro.Field);
        Assert.Equal("Valor final não pode ser negativo.", erro.Message);
    }

    [Fact]
    public async Task AtualizarValorFinalAsync_RetornaNaoEncontradoQuandoMensalidadeNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AtualizarValorFinalAsync(123, 240m);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("Mensalidade não encontrada para atualizar valor.", result.Message);
    }

    [Fact]
    public async Task AlterarStatusMensalidadeAsync_DefineDataPagamentoQuandoPago()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var mensalidade = CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pendente);

        dbContext.Add(aluno);
        await dbContext.SaveChangesAsync();
        mensalidade.AlunoId = aluno.Id;
        dbContext.Mensalidades.Add(mensalidade);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.AlterarStatusMensalidadeAsync(mensalidade.Id, StatusMensalidadeEnum.Pago);

        var atualizada = await dbContext.Mensalidades.FindAsync(mensalidade.Id);
        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Status da mensalidade atualizado.", result.Message);
        Assert.Equal(StatusMensalidadeEnum.Pago, atualizada?.Status);
        Assert.Equal(TestClock.FixedTodayDate, atualizada?.DataPagamento);
    }

    [Theory]
    [InlineData(StatusMensalidadeEnum.Pendente)]
    [InlineData(StatusMensalidadeEnum.Cancelado)]
    public async Task AlterarStatusMensalidadeAsync_LimpaDataPagamentoQuandoReabreOuCancela(StatusMensalidadeEnum status)
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var mensalidade = CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pago);
        mensalidade.DataPagamento = new DateOnly(2026, 7, 8);

        dbContext.Add(aluno);
        await dbContext.SaveChangesAsync();
        mensalidade.AlunoId = aluno.Id;
        dbContext.Mensalidades.Add(mensalidade);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.AlterarStatusMensalidadeAsync(mensalidade.Id, status);

        var atualizada = await dbContext.Mensalidades.FindAsync(mensalidade.Id);
        Assert.True(result.Success);
        Assert.Equal(status, atualizada?.Status);
        Assert.Null(atualizada?.DataPagamento);
    }

    [Fact]
    public async Task AlterarStatusMensalidadeAsync_RetornaNaoEncontradoQuandoMensalidadeNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AlterarStatusMensalidadeAsync(123, StatusMensalidadeEnum.Pago);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("Mensalidade não encontrada para alterar status.", result.Message);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static FinanceiroService CriarService(ApplicationDbContext dbContext)
    {
        return new FinanceiroService(
            dbContext,
            new TestClock(),
            new RecordingAuditLogger(),
            new StubCurrentUserService());
    }

    private static Aluno CriarAluno()
    {
        return new Aluno
        {
            NomeCompleto = "Ana Mori",
            CPF = "12345678901",
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo
        };
    }

    private static Mensalidade CriarMensalidade(int alunoId, StatusMensalidadeEnum status)
    {
        return new Mensalidade
        {
            AlunoId = alunoId,
            Competencia = new DateOnly(2026, 7, 1),
            DataVencimento = new DateOnly(2026, 7, 10),
            ValorBase = 260m,
            ValorFinal = 260m,
            Status = status
        };
    }

    private sealed class TestClock : IClock
    {
        private static readonly DateTime FixedUtcNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);
        public static readonly DateOnly FixedTodayDate = new(2026, 7, 13);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedUtcNow.ToLocalTime();
        public DateTime Today => FixedUtcNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
