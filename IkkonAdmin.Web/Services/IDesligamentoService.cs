using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IDesligamentoService
{
    Task<OperationResult<int>> CriarAsync(Desligamento desligamento, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarAsync(
        int id,
        string motivo,
        decimal pendenciaFinanceira,
        decimal multaRescisoria,
        bool requerimentoRecebido,
        bool acessosRemovidos,
        string? observacoes,
        CancellationToken cancellationToken = default);

    Task<OperationResult<DesligamentoConfirmacaoResultado>> ConfirmarAsync(
        int id,
        bool encerrarCobrancasFuturas,
        CancellationToken cancellationToken = default);
}

public sealed class DesligamentoConfirmacaoResultado
{
    public int CobrancasCanceladas { get; set; }
    public int AlunoId { get; set; }
}
