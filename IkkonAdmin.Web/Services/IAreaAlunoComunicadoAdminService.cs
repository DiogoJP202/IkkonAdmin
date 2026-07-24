using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoComunicadoAdminService
{
    Task<AreaAlunoComunicadosAdminViewModel> ObterComunicadosAsync(CancellationToken cancellationToken = default);
    Task<int> ContarComunicadosAtivosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AreaAlunoComunicadoAdminItemViewModel>> ListarComunicadosRecentesAsync(
        int limite,
        CancellationToken cancellationToken = default);
    Task<OperationResult> CriarComunicadoAsync(
        ComunicadoFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarComunicadoAsync(
        int id,
        ComunicadoFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirComunicadoAsync(
        int id,
        CancellationToken cancellationToken = default);
}
