using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class DesligamentoService(
    ApplicationDbContext dbContext,
    IClock clock) : IDesligamentoService
{
    public async Task<OperationResult<int>> CriarAsync(
        Desligamento desligamento,
        CancellationToken cancellationToken = default)
    {
        desligamento.Motivo = desligamento.Motivo.Trim();
        desligamento.Observacoes = LimparOpcional(desligamento.Observacoes);

        var existeAberto = await dbContext.Desligamentos
            .AnyAsync(x => x.AlunoId == desligamento.AlunoId && !x.DataConfirmacao.HasValue, cancellationToken);

        if (existeAberto)
        {
            return OperationResult<int>.Fail("Já existe um processo de desligamento em aberto para este aluno.");
        }

        await dbContext.Desligamentos.AddAsync(desligamento, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(desligamento.Id, "Solicitação de desligamento criada com sucesso.");
    }

    public async Task<OperationResult> AtualizarAsync(
        int id,
        string motivo,
        decimal pendenciaFinanceira,
        decimal multaRescisoria,
        bool requerimentoRecebido,
        bool acessosRemovidos,
        string? observacoes,
        CancellationToken cancellationToken = default)
    {
        var desligamento = await dbContext.Desligamentos
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (desligamento is null)
        {
            return OperationResult.NotFound("Desligamento não encontrado.");
        }

        desligamento.Motivo = motivo.Trim();
        desligamento.PendenciaFinanceira = decimal.Round(pendenciaFinanceira, 2);
        desligamento.MultaRescisoria = decimal.Round(multaRescisoria, 2);
        desligamento.RequerimentoRecebido = requerimentoRecebido;
        desligamento.AcessosRemovidos = acessosRemovidos;
        desligamento.Observacoes = LimparOpcional(observacoes);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Processo de desligamento atualizado.");
    }

    public async Task<OperationResult<DesligamentoConfirmacaoResultado>> ConfirmarAsync(
        int id,
        bool encerrarCobrancasFuturas,
        CancellationToken cancellationToken = default)
    {
        var desligamento = await dbContext.Desligamentos
            .Include(x => x.Aluno)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (desligamento is null)
        {
            return OperationResult<DesligamentoConfirmacaoResultado>.NotFound("Desligamento não encontrado.");
        }

        if (desligamento.DataConfirmacao.HasValue)
        {
            return OperationResult<DesligamentoConfirmacaoResultado>.Fail("Desligamento já confirmado.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        desligamento.DataConfirmacao = clock.TodayDate;

        if (desligamento.Aluno is not null)
        {
            desligamento.Aluno.Status = StatusAlunoEnum.Desligado;
        }

        var cobrancasCanceladas = 0;
        if (encerrarCobrancasFuturas)
        {
            cobrancasCanceladas = await EncerrarCobrancasFuturasAsync(desligamento.AlunoId, cancellationToken);
        }

        dbContext.HistoricosAlunos.Add(new HistoricoAluno
        {
            AlunoId = desligamento.AlunoId,
            DataEvento = clock.Now,
            TipoEvento = "Desligamento",
            Descricao =
                $"Desligamento confirmado. Pendencia: {desligamento.PendenciaFinanceira:C}. Multa: {desligamento.MultaRescisoria:C}. Cobrancas futuras canceladas: {cobrancasCanceladas}."
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return OperationResult<DesligamentoConfirmacaoResultado>.Ok(
            new DesligamentoConfirmacaoResultado
            {
                CobrancasCanceladas = cobrancasCanceladas,
                AlunoId = desligamento.AlunoId
            },
            "Desligamento confirmado.");
    }

    private async Task<int> EncerrarCobrancasFuturasAsync(int alunoId, CancellationToken cancellationToken)
    {
        var hoje = clock.TodayDate;
        var competenciaAtual = new DateOnly(hoje.Year, hoje.Month, 1);

        var futuras = await dbContext.Mensalidades
            .Where(x => x.AlunoId == alunoId &&
                        x.Competencia > competenciaAtual &&
                        x.Status == StatusMensalidadeEnum.Pendente)
            .ToListAsync(cancellationToken);

        foreach (var mensalidade in futuras)
        {
            mensalidade.Status = StatusMensalidadeEnum.Cancelado;
            mensalidade.Observacoes = AppendObservacao(mensalidade.Observacoes, "Cancelada por desligamento do aluno.");
        }

        if (futuras.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return futuras.Count;
    }

    private static string? LimparOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }

    private static string AppendObservacao(string? baseObservacao, string complemento)
    {
        if (string.IsNullOrWhiteSpace(baseObservacao))
        {
            return complemento;
        }

        return $"{baseObservacao.Trim()} {complemento}";
    }
}
