using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogLookupService
{
    Task<List<BlogCategorySelectItemViewModel>> ListCategoriesForFilterAsync(CancellationToken cancellationToken = default);
    Task<List<BlogCategorySelectItemViewModel>> ListCategoriesForFormAsync(int? currentCategoryId, CancellationToken cancellationToken = default);
    Task<List<BlogAuthorSelectItemViewModel>> ListAuthorsAsync(CancellationToken cancellationToken = default);
    Task<List<string>> ListTagSuggestionsAsync(CancellationToken cancellationToken = default);
    Task<UsuarioSistema?> GetValidAuthorAsync(int? authorUserId, CancellationToken cancellationToken = default);
    Task<bool> IsCategoryValidAsync(int? categoryId, CancellationToken cancellationToken = default);
}
