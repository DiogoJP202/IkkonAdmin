using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogAdminQueryService
{
    Task<BlogAdminIndexViewModel> ListarAsync(BlogAdminFilterViewModel filtro, CancellationToken cancellationToken = default);
    Task<BlogPostFormViewModel> ObterFormCriacaoAsync(int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<BlogPostFormViewModel?> ObterFormEdicaoAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogPreviewViewModel?> ObterPreviewAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogVersionOverviewViewModel?> ObterVersoesAsync(int id, CancellationToken cancellationToken = default);
}
