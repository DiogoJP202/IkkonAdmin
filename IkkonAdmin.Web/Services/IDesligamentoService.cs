using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IDesligamentoService
{
    Task<IReadOnlyList<Desligamento>> ListarAsync(
        string? busca = null,
        bool? confirmado = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aluno>> ListarAlunosElegiveisAsync(CancellationToken cancellationToken = default);
    Task<Desligamento?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default);
    Task<decimal> CalcularPendenciasAsync(int alunoId, CancellationToken cancellationToken = default);
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
