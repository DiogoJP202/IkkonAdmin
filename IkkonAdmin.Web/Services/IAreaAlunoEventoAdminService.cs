using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoEventoAdminService
{
    Task<AreaAlunoEventosAdminViewModel> ObterEventosAsync(EventoAdminFilter filter, CancellationToken cancellationToken = default);
    Task<int> ContarEventosProximosAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CriarEventoAsync(
        EventoAlunoFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarEventoAsync(
        int id,
        EventoAlunoFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirEventoAsync(
        int id,
        CancellationToken cancellationToken = default);
}
