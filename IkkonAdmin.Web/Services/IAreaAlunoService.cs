using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoService
{
    Task<AreaAlunoDashboardViewModel?> ObterDashboardAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoPerfilViewModel?> ObterPerfilAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoFinanceiroViewModel?> ObterFinanceiroAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoTurmasViewModel?> ObterTurmasAsync(int usuarioId, CancellationToken cancellationToken = default);
}
