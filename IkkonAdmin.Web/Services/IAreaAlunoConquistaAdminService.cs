using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoConquistaAdminService
{
    Task<AreaAlunoConquistasAdminViewModel> ObterConquistasAsync(ConquistaAdminFilter filter, CancellationToken cancellationToken = default);
    Task<int> ContarConquistasConcedidasAsync(
        DateTime inicioUtc,
        DateTime fimUtc,
        CancellationToken cancellationToken = default);
    Task<OperationResult> CriarInsigniaAsync(
        InsigniaFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarInsigniaAsync(
        int id,
        InsigniaFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirInsigniaAsync(
        int id,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AtribuirInsigniaAsync(
        AlunoInsigniaFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarAlunoInsigniaAsync(
        int id,
        AlunoInsigniaFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirAlunoInsigniaAsync(
        int id,
        CancellationToken cancellationToken = default);
}
