using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoFrequenciaService
{
    Task<AreaAlunoFrequenciaViewModel?> ObterFrequenciaAsync(int usuarioId, DateOnly? inicio, DateOnly? fim, CancellationToken cancellationToken = default);
    Task<AreaAlunoResumoFrequencia> ObterResumoFrequenciaAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<List<AreaAlunoFrequenciaItemViewModel>> ListarFaltasRecentesAsync(int alunoId, int limite, CancellationToken cancellationToken = default);
}
