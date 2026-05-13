using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogService
{
    Task<BlogAdminIndexViewModel> ListarAsync(BlogAdminFilterViewModel filtro, CancellationToken cancellationToken = default);
    Task<BlogPostFormViewModel> ObterFormCriacaoAsync(int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<BlogPostFormViewModel?> ObterFormEdicaoAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogPreviewViewModel?> ObterPreviewAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogOperationResult> CriarAsync(BlogPostFormViewModel model, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<BlogOperationResult> AtualizarAsync(int id, BlogPostFormViewModel model, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<BlogOperationResult> ExcluirAsync(int id, int? usuarioAtualId, CancellationToken cancellationToken = default);
}
