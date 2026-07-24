using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoComunicadosService
{
    Task<AreaAlunoComunicadosViewModel?> ObterComunicadosAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<bool> MarcarComunicadoComoLidoAsync(int usuarioId, int comunicadoId, CancellationToken cancellationToken = default);
    Task<List<AreaAlunoComunicadoItemViewModel>> ListarComunicadosAsync(int alunoId, IReadOnlyCollection<int> turmaIds, int limite, CancellationToken cancellationToken = default);
}
