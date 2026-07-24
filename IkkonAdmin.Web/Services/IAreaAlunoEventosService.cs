using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoEventosService
{
    Task<AreaAlunoEventosViewModel?> ObterEventosAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<List<AreaAlunoEventoItemViewModel>> ListarEventosAsync(int alunoId, IReadOnlyCollection<int> turmaIds, int limite, CancellationToken cancellationToken = default);
}
