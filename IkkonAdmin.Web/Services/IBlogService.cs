using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogService
{
    Task<BlogAdminIndexViewModel> ListarAsync(BlogAdminFilterViewModel filtro, CancellationToken cancellationToken = default);
    Task<BlogPostFormViewModel> ObterFormCriacaoAsync(int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<BlogPostFormViewModel?> ObterFormEdicaoAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogPreviewViewModel?> ObterPreviewAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogVersionOverviewViewModel?> ObterVersoesAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogPublicIndexViewModel> ListarPublicoAsync(BlogPublicFilterViewModel filtro, CancellationToken cancellationToken = default);
    Task<BlogPublicDetailsViewModel?> ObterPublicoPorSlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> CriarAsync(BlogPostFormViewModel model, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> CriarVersaoAsync(int id, string languageCode, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> AtualizarAsync(int id, BlogPostFormViewModel model, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> ExcluirAsync(int id, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> ExcluirVersaoAsync(int id, int versionId, int? usuarioAtualId, CancellationToken cancellationToken = default);
}
