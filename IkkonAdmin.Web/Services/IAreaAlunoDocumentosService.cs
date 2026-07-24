using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoDocumentosService
{
    Task<AreaAlunoDocumentosViewModel?> ObterDocumentosAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult> EnviarDocumentoAsync(int usuarioId, int solicitacaoId, IFormFile arquivo, CancellationToken cancellationToken = default);
    Task<AreaAlunoDocumentoDownload?> ObterDocumentoParaDownloadAsync(int usuarioId, int envioId, CancellationToken cancellationToken = default);
    Task<List<AreaAlunoDocumentoItemViewModel>> ListarDocumentosAsync(int alunoId, int limite, CancellationToken cancellationToken = default);
}
