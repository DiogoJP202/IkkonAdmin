using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class BlogLookupService(ApplicationDbContext dbContext) : IBlogLookupService
{
    public async Task<List<BlogCategorySelectItemViewModel>> ListCategoriesForFilterAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.BlogCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BlogCategorySelectItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BlogCategorySelectItemViewModel>> ListCategoriesForFormAsync(
        int? currentCategoryId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.IsActive || x.Id == currentCategoryId)
            .OrderBy(x => x.Name)
            .Select(x => new BlogCategorySelectItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<BlogAuthorSelectItemViewModel>> ListAuthorsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UsuariosSistema
            .AsNoTracking()
            .Where(x => x.Ativo && x.TipoAcesso != TipoAcessoEnum.Aluno)
            .OrderBy(x => x.NomeExibicao)
            .Select(x => new BlogAuthorSelectItemViewModel
            {
                Id = x.Id,
                Nome = x.NomeExibicao
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> ListTagSuggestionsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.BlogTags
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    public async Task<UsuarioSistema?> GetValidAuthorAsync(
        int? authorUserId,
        CancellationToken cancellationToken = default)
    {
        if (!authorUserId.HasValue)
        {
            return null;
        }

        return await dbContext.UsuariosSistema
            .FirstOrDefaultAsync(
                x => x.Id == authorUserId.Value &&
                     x.Ativo &&
                     x.TipoAcesso != TipoAcessoEnum.Aluno,
                cancellationToken);
    }

    public async Task<bool> IsCategoryValidAsync(
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        if (!categoryId.HasValue)
        {
            return true;
        }

        return await dbContext.BlogCategories
            .AnyAsync(x => x.Id == categoryId.Value && x.IsActive, cancellationToken);
    }
}
