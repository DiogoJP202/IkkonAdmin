using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoConquistasService
{
    Task<AreaAlunoConquistasViewModel?> ObterConquistasAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<List<AreaAlunoConquistaItemViewModel>> ListarConquistasAsync(int alunoId, int limite, CancellationToken cancellationToken = default);
    Task GarantirConquistasAutomaticasAsync(int alunoId, CancellationToken cancellationToken = default);
}
