using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoFinanceiroService
{
    Task<AreaAlunoFinanceiroViewModel?> ObterFinanceiroAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<List<AreaAlunoMensalidadeItemViewModel>> ListarMensalidadesAsync(int alunoId, int limite, CancellationToken cancellationToken = default);
    Task<AreaAlunoResumoFinanceiro> ObterResumoFinanceiroAsync(int alunoId, CancellationToken cancellationToken = default);
}
