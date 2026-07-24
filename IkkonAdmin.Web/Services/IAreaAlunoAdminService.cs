using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoAdminService
{
    Task<AreaAlunoAdminDashboardViewModel> ObterDashboardAsync(CancellationToken cancellationToken = default);

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

    Task<AreaAlunoDocumentosAdminViewModel> ObterDocumentosAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CriarDocumentoTipoAsync(DocumentoTipoFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarDocumentoTipoAsync(int id, DocumentoTipoFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirDocumentoTipoAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> SolicitarDocumentoAsync(DocumentoSolicitacaoFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarDocumentoSolicitacaoAsync(int id, DocumentoSolicitacaoEdicaoViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirDocumentoSolicitacaoAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> AvaliarDocumentoAsync(DocumentoAvaliacaoFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoDocumentoDownload?> ObterDocumentoAdminDownloadAsync(int envioId, CancellationToken cancellationToken = default);

    Task<AreaAlunoComunicadosAdminViewModel> ObterComunicadosAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CriarComunicadoAsync(ComunicadoFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarComunicadoAsync(int id, ComunicadoFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirComunicadoAsync(int id, CancellationToken cancellationToken = default);

    Task<AreaAlunoEventosAdminViewModel> ObterEventosAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CriarEventoAsync(EventoAlunoFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarEventoAsync(int id, EventoAlunoFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirEventoAsync(int id, CancellationToken cancellationToken = default);

    Task<AreaAlunoConquistasAdminViewModel> ObterConquistasAsync(CancellationToken cancellationToken = default);
    Task<OperationResult> CriarInsigniaAsync(InsigniaFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarInsigniaAsync(int id, InsigniaFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirInsigniaAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> AtribuirInsigniaAsync(AlunoInsigniaFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarAlunoInsigniaAsync(int id, AlunoInsigniaFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirAlunoInsigniaAsync(int id, CancellationToken cancellationToken = default);
}
