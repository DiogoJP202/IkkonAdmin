using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class BlogPublicService(
    ApplicationDbContext dbContext,
    IBlogLanguageService blogLanguageService) : IBlogPublicService
{
    public async Task<BlogPublicIndexViewModel> ListarPublicoAsync(
        BlogPublicFilterViewModel filtro,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var pageSize = 9;
        var currentPage = Math.Max(1, filtro.Pagina);
        var currentLanguage = blogLanguageService.GetCurrentLanguageCode();
        var query = ApplyPublicFilters(CreatePublicQuery(now), filtro);
        var selectedPosts = await SelectPublicVersionsAsync(query, currentLanguage, cancellationToken);
        var totalPosts = selectedPosts.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalPosts / (decimal)pageSize));

        if (currentPage > totalPages)
        {
            currentPage = totalPages;
        }

        filtro.Pagina = currentPage;

        var pageIds = selectedPosts
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToList();
        var featuredIds = (await SelectPublicVersionsAsync(
                CreatePublicQuery(now).Where(x => x.IsFeatured),
                currentLanguage,
                cancellationToken))
            .Take(3)
            .Select(x => x.Id)
            .ToList();
        var weeklyIds = (await SelectPublicVersionsAsync(
                CreatePublicQuery(now).Where(x => x.IsWeeklyHighlight),
                currentLanguage,
                cancellationToken))
            .Take(2)
            .Select(x => x.Id)
            .ToList();

        return new BlogPublicIndexViewModel
        {
            Filtro = filtro,
            FeaturedPosts = await GetPublicCardsByIdsAsync(now, featuredIds, cancellationToken),
            WeeklyHighlights = await GetPublicCardsByIdsAsync(now, weeklyIds, cancellationToken),
            Posts = await GetPublicCardsByIdsAsync(now, pageIds, cancellationToken),
            Categories = await ListPublicCategoriesAsync(now, filtro.Categoria, cancellationToken),
            Tags = await ListPublicTagsAsync(now, filtro.Tag, cancellationToken),
            TotalPosts = totalPosts,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<BlogPublicDetailsViewModel?> ObterPublicoPorSlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var normalizedSlug = slug.Trim();
        var currentLanguage = blogLanguageService.GetCurrentLanguageCode();

        var source = await CreatePublicQuery(now)
            .Where(x => x.Slug == normalizedSlug)
            .Select(x => new
            {
                x.Id,
                x.LanguageCode,
                x.TranslationGroupId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            return null;
        }

        var groupId = source.TranslationGroupId ?? source.Id;
        var versionIds = await SelectPublicVersionsAsync(
            CreatePublicQuery(now).Where(x => x.Id == groupId || x.TranslationGroupId == groupId),
            currentLanguage,
            cancellationToken);
        var selectedId = versionIds.FirstOrDefault()?.Id ?? source.Id;

        var post = await CreatePublicQuery(now)
            .Where(x => x.Id == selectedId)
            .Select(x => new BlogPublicDetailsViewModel
            {
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                ContentHtml = x.ContentHtml ?? string.Empty,
                CoverImageUrl = x.CoverImageUrl,
                AuthorName = x.AuthorDisplayName,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CategorySlug = x.Category != null ? x.Category.Slug : null,
                PublishedAtUtc = x.PublishedAtUtc ?? x.CreatedAtUtc,
                ReadingTimeMinutes = x.ReadingTimeMinutes,
                SeoTitle = x.SeoTitle,
                SeoDescription = x.SeoDescription,
                LanguageCode = x.LanguageCode,
                UpdatedAtUtc = x.UpdatedAtUtc ?? x.PublishedAtUtc ?? x.CreatedAtUtc,
                Tags = x.PostTags
                    .OrderBy(t => t.BlogTag.Name)
                    .Select(t => new BlogPublicTagViewModel
                    {
                        Name = t.BlogTag.Name,
                        Slug = t.BlogTag.Slug
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (post is null)
        {
            return null;
        }

        post.AlternateVersions = await CreatePublicQuery(now)
            .Where(x => x.Id == groupId || x.TranslationGroupId == groupId)
            .OrderBy(x => x.Id)
            .Select(x => new BlogPublicAlternateVersionViewModel
            {
                LanguageCode = x.LanguageCode,
                Slug = x.Slug
            })
            .ToListAsync(cancellationToken);

        var relatedQuery = CreatePublicQuery(now)
            .Where(x => (x.TranslationGroupId ?? x.Id) != groupId);

        if (!string.IsNullOrWhiteSpace(post.CategorySlug))
        {
            relatedQuery = relatedQuery.Where(x => x.Category != null && x.Category.Slug == post.CategorySlug);
        }

        var relatedSelections = (await SelectPublicVersionsAsync(relatedQuery, currentLanguage, cancellationToken))
            .Take(3)
            .ToList();
        var relatedPosts = await GetPublicCardsByIdsAsync(
            now,
            relatedSelections.Select(x => x.Id).ToList(),
            cancellationToken);

        if (relatedPosts.Count < 3)
        {
            var excludedGroupIds = relatedSelections
                .Select(x => x.GroupId)
                .Append(groupId)
                .ToList();

            var fallbackSelections = (await SelectPublicVersionsAsync(
                    CreatePublicQuery(now)
                        .Where(x => !excludedGroupIds.Contains(x.TranslationGroupId ?? x.Id)),
                    currentLanguage,
                    cancellationToken))
                .Take(3 - relatedPosts.Count)
                .ToList();
            var fallbackPosts = await GetPublicCardsByIdsAsync(
                now,
                fallbackSelections.Select(x => x.Id).ToList(),
                cancellationToken);

            relatedPosts.AddRange(fallbackPosts);
        }

        post.RelatedPosts = relatedPosts;
        return post;
    }

    private async Task<List<BlogPublicPostSelection>> SelectPublicVersionsAsync(
        IQueryable<BlogPost> query,
        string languageCode,
        CancellationToken cancellationToken)
    {
        var candidates = await query
            .Select(x => new BlogPublicPostSelection(
                x.Id,
                x.TranslationGroupId ?? x.Id,
                x.LanguageCode,
                x.PublishedAtUtc,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return candidates
            .GroupBy(x => x.GroupId)
            .Select(group => PickBestPublicVersion(group, languageCode))
            .OrderByDescending(x => x.PublishedAtUtc ?? x.CreatedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToList();
    }

    private async Task<List<BlogPublicPostCardViewModel>> GetPublicCardsByIdsAsync(
        DateTime nowUtc,
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var order = ids
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);
        var cards = await SelectPublicCards(CreatePublicQuery(nowUtc).Where(x => ids.Contains(x.Id)))
            .ToListAsync(cancellationToken);

        return cards
            .OrderBy(x => order.TryGetValue(x.Id, out var index) ? index : int.MaxValue)
            .ToList();
    }

    private IQueryable<BlogPost> CreatePublicQuery(DateTime nowUtc)
    {
        return dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.DeletedAtUtc == null &&
                        x.Status == BlogPostStatusEnum.Published &&
                        x.PublishedAtUtc.HasValue &&
                        x.PublishedAtUtc <= nowUtc)
            .OrderByDescending(x => x.PublishedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc);
    }

    private static IQueryable<BlogPost> ApplyPublicFilters(
        IQueryable<BlogPost> query,
        BlogPublicFilterViewModel filtro)
    {
        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var termo = filtro.Q.Trim();
            query = query.Where(x =>
                x.Title.Contains(termo) ||
                (x.Summary != null && x.Summary.Contains(termo)) ||
                (x.ContentText != null && x.ContentText.Contains(termo)) ||
                (x.Category != null && x.Category.Name.Contains(termo)) ||
                x.PostTags.Any(t => t.BlogTag.Name.Contains(termo)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
        {
            var categoriaSlug = filtro.Categoria.Trim();
            query = query.Where(x => x.Category != null && x.Category.Slug == categoriaSlug);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tag))
        {
            var tagSlug = filtro.Tag.Trim();
            query = query.Where(x => x.PostTags.Any(t => t.BlogTag.Slug == tagSlug));
        }

        return query;
    }

    private static IQueryable<BlogPublicPostCardViewModel> SelectPublicCards(IQueryable<BlogPost> query)
    {
        return query.Select(x => new BlogPublicPostCardViewModel
        {
            Id = x.Id,
            Title = x.Title,
            Slug = x.Slug,
            Summary = x.Summary,
            CoverImageUrl = x.CoverImageUrl,
            AuthorName = x.AuthorDisplayName,
            CategoryName = x.Category != null ? x.Category.Name : null,
            CategorySlug = x.Category != null ? x.Category.Slug : null,
            PublishedAtUtc = x.PublishedAtUtc ?? x.CreatedAtUtc,
            ReadingTimeMinutes = x.ReadingTimeMinutes,
            IsFeatured = x.IsFeatured,
            IsWeeklyHighlight = x.IsWeeklyHighlight,
            Tags = x.PostTags
                .OrderBy(t => t.BlogTag.Name)
                .Select(t => new BlogPublicTagViewModel
                {
                    Name = t.BlogTag.Name,
                    Slug = t.BlogTag.Slug
                })
                .Take(5)
                .ToList()
        });
    }

    private async Task<List<BlogPublicTaxonomyItemViewModel>> ListPublicCategoriesAsync(
        DateTime nowUtc,
        string? selectedSlug,
        CancellationToken cancellationToken)
    {
        var categorias = await dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new BlogPublicTaxonomyItemViewModel
            {
                Name = x.Name,
                Slug = x.Slug,
                Count = x.Posts.Count(p =>
                    p.DeletedAtUtc == null &&
                    p.Status == BlogPostStatusEnum.Published &&
                    p.PublishedAtUtc.HasValue &&
                    p.PublishedAtUtc <= nowUtc),
                IsActive = x.Slug == selectedSlug
            })
            .ToListAsync(cancellationToken);

        return categorias.Where(x => x.Count > 0 || x.IsActive).ToList();
    }

    private async Task<List<BlogPublicTaxonomyItemViewModel>> ListPublicTagsAsync(
        DateTime nowUtc,
        string? selectedSlug,
        CancellationToken cancellationToken)
    {
        var tags = await dbContext.BlogTags
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new BlogPublicTaxonomyItemViewModel
            {
                Name = x.Name,
                Slug = x.Slug,
                Count = x.PostTags.Count(t =>
                    t.BlogPost.DeletedAtUtc == null &&
                    t.BlogPost.Status == BlogPostStatusEnum.Published &&
                    t.BlogPost.PublishedAtUtc.HasValue &&
                    t.BlogPost.PublishedAtUtc <= nowUtc),
                IsActive = x.Slug == selectedSlug
            })
            .Take(24)
            .ToListAsync(cancellationToken);

        return tags.Where(x => x.Count > 0 || x.IsActive).ToList();
    }

    private BlogPublicPostSelection PickBestPublicVersion(
        IEnumerable<BlogPublicPostSelection> versions,
        string languageCode)
    {
        var normalizedLanguage = blogLanguageService.Normalize(languageCode);

        return versions
            .OrderByDescending(x => string.Equals(blogLanguageService.Normalize(x.LanguageCode), normalizedLanguage, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(x => string.Equals(blogLanguageService.Normalize(x.LanguageCode), blogLanguageService.DefaultLanguageCode, StringComparison.OrdinalIgnoreCase))
            .ThenBy(x => x.Id == x.GroupId ? 0 : 1)
            .ThenByDescending(x => x.PublishedAtUtc ?? x.CreatedAtUtc)
            .First();
    }

    private sealed record BlogPublicPostSelection(
        int Id,
        int GroupId,
        string LanguageCode,
        DateTime? PublishedAtUtc,
        DateTime CreatedAtUtc);
}
