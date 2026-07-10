using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoAdminService
{
    Task<AreaAlunoAdminDashboardViewModel> ObterDashboardAsync(CancellationToken cancellationToken = default);

    Task<AreaAlunoAulasAdminViewModel> ObterAulasAsync(CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> CriarHorarioAsync(TurmaHorarioFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarHorarioAsync(int id, TurmaHorarioFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirHorarioAsync(int id, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> VincularInstrutorAsync(TurmaInstrutorFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarInstrutorAsync(int id, TurmaInstrutorFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirInstrutorAsync(int id, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> CriarAulaAsync(AulaFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarAulaAsync(int id, AulaFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirAulaAsync(int id, CancellationToken cancellationToken = default);

    Task<AreaAlunoFrequenciaAdminViewModel> ObterFrequenciaAsync(CancellationToken cancellationToken = default);
    Task<AreaAlunoRegistroFrequenciaViewModel?> ObterRegistroFrequenciaAsync(int aulaId, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> SalvarFrequenciaAsync(FrequenciaRegistroPostViewModel model, int? usuarioId, CancellationToken cancellationToken = default);

    Task<AreaAlunoDocumentosAdminViewModel> ObterDocumentosAsync(CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> CriarDocumentoTipoAsync(DocumentoTipoFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarDocumentoTipoAsync(int id, DocumentoTipoFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirDocumentoTipoAsync(int id, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> SolicitarDocumentoAsync(DocumentoSolicitacaoFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarDocumentoSolicitacaoAsync(int id, DocumentoSolicitacaoEdicaoViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirDocumentoSolicitacaoAsync(int id, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AvaliarDocumentoAsync(DocumentoAvaliacaoFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoDocumentoDownload?> ObterDocumentoAdminDownloadAsync(int envioId, CancellationToken cancellationToken = default);

    Task<AreaAlunoComunicadosAdminViewModel> ObterComunicadosAsync(CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> CriarComunicadoAsync(ComunicadoFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarComunicadoAsync(int id, ComunicadoFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirComunicadoAsync(int id, CancellationToken cancellationToken = default);

    Task<AreaAlunoEventosAdminViewModel> ObterEventosAsync(CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> CriarEventoAsync(EventoAlunoFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarEventoAsync(int id, EventoAlunoFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirEventoAsync(int id, CancellationToken cancellationToken = default);

    Task<AreaAlunoConquistasAdminViewModel> ObterConquistasAsync(CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> CriarInsigniaAsync(InsigniaFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarInsigniaAsync(int id, InsigniaFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirInsigniaAsync(int id, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtribuirInsigniaAsync(AlunoInsigniaFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> AtualizarAlunoInsigniaAsync(int id, AlunoInsigniaFormViewModel model, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> ExcluirAlunoInsigniaAsync(int id, CancellationToken cancellationToken = default);
}
