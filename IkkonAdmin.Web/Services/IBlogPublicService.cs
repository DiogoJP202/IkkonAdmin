using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogPublicService
{
    Task<BlogPublicIndexViewModel> ListarPublicoAsync(BlogPublicFilterViewModel filtro, CancellationToken cancellationToken = default);
    Task<BlogPublicDetailsViewModel?> ObterPublicoPorSlugAsync(string slug, CancellationToken cancellationToken = default);
}
