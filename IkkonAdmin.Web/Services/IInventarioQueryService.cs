using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IInventarioQueryService
{
    Task<InventarioIndexViewModel> ListarAsync(
        InventarioFiltroViewModel filtro,
        CancellationToken cancellationToken = default);

    Task<InventarioDetalhesViewModel?> ObterDetalhesAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<InventarioFormViewModel> ObterFormCriacaoAsync(CancellationToken cancellationToken = default);

    Task<InventarioFormViewModel?> ObterFormEdicaoAsync(
        int id,
        CancellationToken cancellationToken = default);
}
