using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoDocumentoAdminService
{
    Task<AreaAlunoDocumentosAdminViewModel> ObterDocumentosAsync(CancellationToken cancellationToken = default);
    Task<int> ContarDocumentosPendentesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<AreaAlunoDocumentoAdminItemViewModel>> ListarDocumentosRecentesAsync(
        int limite,
        CancellationToken cancellationToken = default);
    Task<OperationResult> CriarDocumentoTipoAsync(
        DocumentoTipoFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarDocumentoTipoAsync(
        int id,
        DocumentoTipoFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirDocumentoTipoAsync(
        int id,
        CancellationToken cancellationToken = default);
    Task<OperationResult> SolicitarDocumentoAsync(
        DocumentoSolicitacaoFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarDocumentoSolicitacaoAsync(
        int id,
        DocumentoSolicitacaoEdicaoViewModel model,
        CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirDocumentoSolicitacaoAsync(
        int id,
        CancellationToken cancellationToken = default);
    Task<OperationResult> AvaliarDocumentoAsync(
        DocumentoAvaliacaoFormViewModel model,
        CancellationToken cancellationToken = default);
    Task<AreaAlunoDocumentoDownload?> ObterDocumentoAdminDownloadAsync(
        int envioId,
        CancellationToken cancellationToken = default);
}
