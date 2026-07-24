using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IInventarioService
{
    Task<InventarioIndexViewModel> ListarAsync(InventarioFiltroViewModel filtro, CancellationToken cancellationToken = default);
    Task<InventarioDetalhesViewModel?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default);
    Task<InventarioFormViewModel> ObterFormCriacaoAsync(CancellationToken cancellationToken = default);
    Task<InventarioFormViewModel?> ObterFormEdicaoAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> CriarAsync(InventarioFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> AtualizarAsync(int id, InventarioFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> InativarAsync(int id, int? usuarioId, CancellationToken cancellationToken = default);
}
