using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogService
{
    Task<OperationResult<int>> CriarAsync(BlogPostFormViewModel model, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> CriarVersaoAsync(int id, string languageCode, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> AtualizarAsync(int id, BlogPostFormViewModel model, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> ExcluirAsync(int id, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> ExcluirVersaoAsync(int id, int versionId, int? usuarioAtualId, CancellationToken cancellationToken = default);
}
