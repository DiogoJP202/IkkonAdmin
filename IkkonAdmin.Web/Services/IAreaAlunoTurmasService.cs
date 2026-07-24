using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoTurmasService
{
    Task<AreaAlunoTurmasViewModel?> ObterTurmasAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoAulasViewModel?> ObterAulasAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<List<AreaAlunoTurmaItemViewModel>> ListarTurmasAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<List<AreaAlunoAulaItemViewModel>> ListarProximasAulasAsync(IReadOnlyCollection<int> turmaIds, int limite, CancellationToken cancellationToken = default);
}
