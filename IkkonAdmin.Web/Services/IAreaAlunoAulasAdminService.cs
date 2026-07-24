using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoAulasAdminService
{
    Task<AreaAlunoAulasAdminViewModel> ObterAulasAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CriarHorarioAsync(TurmaHorarioFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarHorarioAsync(int id, TurmaHorarioFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirHorarioAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> VincularInstrutorAsync(TurmaInstrutorFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarInstrutorAsync(int id, TurmaInstrutorFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirInstrutorAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CriarAulaAsync(AulaFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarAulaAsync(int id, AulaFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirAulaAsync(int id, CancellationToken cancellationToken = default);

    Task<AreaAlunoFrequenciaAdminViewModel> ObterFrequenciaAsync(CancellationToken cancellationToken = default);
    Task<AreaAlunoRegistroFrequenciaViewModel?> ObterRegistroFrequenciaAsync(int aulaId, CancellationToken cancellationToken = default);
    Task<OperationResult> SalvarFrequenciaAsync(FrequenciaRegistroPostViewModel model, int? usuarioId, CancellationToken cancellationToken = default);

    Task<int> ContarAulasProximasAsync(DateTime inicioMinimo, CancellationToken cancellationToken = default);
    Task<int> ContarFrequenciasRegistradasAsync(DateTime inicio, DateTime fim, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AreaAlunoAulaAdminItemViewModel>> ListarAulasAdminAsync(int limite, DateTime inicioMinimo, CancellationToken cancellationToken = default);
}
