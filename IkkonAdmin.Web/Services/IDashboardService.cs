using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> ObterDashboardAsync(
        int? anoReferencia = null,
        int? mesReferencia = null,
        int? turmaId = null,
        CancellationToken cancellationToken = default);
}
