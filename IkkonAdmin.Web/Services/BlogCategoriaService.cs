using System.Globalization;
using System.Text;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class BlogCategoriaService(ApplicationDbContext dbContext) : IBlogCategoriaService
{
    public async Task<BlogCategoryIndexViewModel> ListarAsync(CancellationToken cancellationToken = default)
    {
        var categorias = await dbContext.BlogCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BlogCategoryListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc,
                TotalPosts = x.Posts.Count(p => p.DeletedAtUtc == null)
            })
            .ToListAsync(cancellationToken);

        return new BlogCategoryIndexViewModel
        {
            TotalCategories = categorias.Count,
            ActiveCategories = categorias.Count(x => x.IsActive),
            InactiveCategories = categorias.Count(x => !x.IsActive),
            Categories = categorias
        };
    }

    public Task<BlogCategoryFormViewModel> ObterParaCriacaoAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new BlogCategoryFormViewModel
        {
            IsActive = true
        });
    }

    public async Task<BlogCategoryFormViewModel?> ObterParaEdicaoAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BlogCategoryFormViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<BlogCategorySelectItemViewModel>> ListarOpcoesAtivasAsync(
        int? categoriaAtualId = null,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.IsActive || x.Id == categoriaAtualId)
            .OrderBy(x => x.Name)
            .Select(x => new BlogCategorySelectItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<BlogOperationResult> CriarAsync(BlogCategoryFormViewModel model, CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizarTexto(model.Name);
        var slug = await GarantirSlugUnicoAsync(GerarSlug(model.Slug, model.Name), null, cancellationToken);

        if (await dbContext.BlogCategories.AnyAsync(x => x.Name == normalizedName, cancellationToken))
        {
            return BlogOperationResult.Fail("Ja existe uma categoria com esse nome.");
        }

        var categoria = new BlogCategory
        {
            Name = normalizedName,
            Slug = slug,
            Description = LimparOpcional(model.Description),
            IsActive = model.IsActive,
            CreatedAtUtc = DateTime.UtcNow
        };

        await dbContext.BlogCategories.AddAsync(categoria, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return BlogOperationResult.Ok("Categoria criada com sucesso.", categoria.Id);
    }

    public async Task<BlogOperationResult> AtualizarAsync(int id, BlogCategoryFormViewModel model, CancellationToken cancellationToken = default)
    {
        var categoria = await dbContext.BlogCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (categoria is null)
        {
            return BlogOperationResult.Fail("Categoria nao encontrada.");
        }

        var normalizedName = NormalizarTexto(model.Name);
        var slug = await GarantirSlugUnicoAsync(GerarSlug(model.Slug, model.Name), id, cancellationToken);

        if (await dbContext.BlogCategories.AnyAsync(x => x.Id != id && x.Name == normalizedName, cancellationToken))
        {
            return BlogOperationResult.Fail("Ja existe uma categoria com esse nome.");
        }

        categoria.Name = normalizedName;
        categoria.Slug = slug;
        categoria.Description = LimparOpcional(model.Description);
        categoria.IsActive = model.IsActive;
        categoria.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return BlogOperationResult.Ok("Categoria atualizada com sucesso.", categoria.Id);
    }

    public async Task<BlogOperationResult> AlterarStatusAsync(int id, bool ativo, CancellationToken cancellationToken = default)
    {
        var categoria = await dbContext.BlogCategories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (categoria is null)
        {
            return BlogOperationResult.Fail("Categoria nao encontrada.");
        }

        categoria.IsActive = ativo;
        categoria.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogOperationResult.Ok(ativo ? "Categoria ativada com sucesso." : "Categoria desativada com sucesso.", categoria.Id);
    }

    public async Task<BlogOperationResult> ExcluirAsync(int id, CancellationToken cancellationToken = default)
    {
        var categoria = await dbContext.BlogCategories
            .Include(x => x.Posts)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (categoria is null)
        {
            return BlogOperationResult.Fail("Categoria nao encontrada.");
        }

        if (categoria.Posts.Any(x => x.DeletedAtUtc == null))
        {
            return BlogOperationResult.Fail("Esta categoria possui posts vinculados. Desative a categoria para manter o historico dos posts.");
        }

        dbContext.BlogCategories.Remove(categoria);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogOperationResult.Ok("Categoria excluida com sucesso.", categoria.Id);
    }

    private async Task<string> GarantirSlugUnicoAsync(string baseSlug, int? ignorarId, CancellationToken cancellationToken)
    {
        var slug = baseSlug;
        var suffix = 2;

        while (await dbContext.BlogCategories.AnyAsync(
                   x => x.Slug == slug && (!ignorarId.HasValue || x.Id != ignorarId.Value),
                   cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string GerarSlug(string? slug, string fallback)
    {
        var source = string.IsNullOrWhiteSpace(slug) ? fallback : slug;
        var normalized = source.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
                continue;
            }

            if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        return builder.ToString().Trim('-');
    }

    private static string NormalizarTexto(string value)
    {
        return value.Trim();
    }

    private static string? LimparOpcional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
