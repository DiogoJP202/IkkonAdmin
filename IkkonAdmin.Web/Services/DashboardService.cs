using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public class DashboardService(IDashboardQueryService queryService) : IDashboardService
{
    public Task<DashboardViewModel> ObterDashboardAsync(
        int? anoReferencia = null,
        int? mesReferencia = null,
        int? turmaId = null,
        CancellationToken cancellationToken = default)
    {
        return queryService.ObterDashboardAsync(anoReferencia, mesReferencia, turmaId, cancellationToken);
    }
}
