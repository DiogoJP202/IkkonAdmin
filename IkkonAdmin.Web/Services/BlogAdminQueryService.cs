using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class BlogAdminQueryService(
    ApplicationDbContext dbContext,
    IBlogWorkflowService blogWorkflowService,
    IBlogLookupService blogLookupService,
    IBlogLanguageService blogLanguageService,
    IBlogDateTimeService blogDateTimeService) : IBlogAdminQueryService
{
    public async Task<BlogAdminIndexViewModel> ListarAsync(
        BlogAdminFilterViewModel filtro,
        CancellationToken cancellationToken = default)
    {
        await blogWorkflowService.PromoteScheduledPostsAsync(cancellationToken);

        var baseQuery = dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.DeletedAtUtc == null);

        var query = baseQuery;

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = filtro.Busca.Trim();
            query = query.Where(x =>
                x.Title.Contains(termo) ||
                (x.Summary != null && x.Summary.Contains(termo)) ||
                (x.ContentText != null && x.ContentText.Contains(termo)) ||
                (x.Category != null && x.Category.Name.Contains(termo)) ||
                x.PostTags.Any(t => t.BlogTag.Name.Contains(termo)));
        }

        if (filtro.Status.HasValue)
        {
            query = query.Where(x => x.Status == filtro.Status.Value);
        }

        if (filtro.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filtro.CategoryId.Value);
        }

        if (filtro.AuthorUserId.HasValue)
        {
            query = query.Where(x => x.AuthorUserId == filtro.AuthorUserId.Value);
        }

        if (filtro.IsFeatured.HasValue)
        {
            query = query.Where(x => x.IsFeatured == filtro.IsFeatured.Value);
        }

        if (filtro.IsWeeklyHighlight.HasValue)
        {
            query = query.Where(x => x.IsWeeklyHighlight == filtro.IsWeeklyHighlight.Value);
        }

        if (filtro.PublishedFrom.HasValue)
        {
            var fromUtc = blogDateTimeService.ConvertSaoPauloDateOnlyToUtcStart(filtro.PublishedFrom.Value);
            query = query.Where(x => x.PublishedAtUtc >= fromUtc);
        }

        if (filtro.PublishedTo.HasValue)
        {
            var toUtcExclusive = blogDateTimeService.ConvertSaoPauloDateOnlyToUtcEndExclusive(filtro.PublishedTo.Value);
            query = query.Where(x => x.PublishedAtUtc < toUtcExclusive);
        }

        var posts = await query
            .OrderByDescending(x => x.PublishedAtUtc ?? x.ScheduledAtUtc ?? x.CreatedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => new BlogPostListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                LanguageCode = x.LanguageCode,
                TranslationVersionCount = dbContext.BlogPosts.Count(v =>
                    v.DeletedAtUtc == null &&
                    (v.Id == (x.TranslationGroupId ?? x.Id) || v.TranslationGroupId == (x.TranslationGroupId ?? x.Id))),
                Status = x.Status,
                CategoryName = x.Category != null ? x.Category.Name : null,
                AuthorName = x.AuthorDisplayName,
                CreatedAtUtc = x.CreatedAtUtc,
                PublishedAtUtc = x.PublishedAtUtc,
                ScheduledAtUtc = x.ScheduledAtUtc,
                IsFeatured = x.IsFeatured,
                IsWeeklyHighlight = x.IsWeeklyHighlight,
                TagCount = x.PostTags.Count
            })
            .ToListAsync(cancellationToken);

        foreach (var post in posts)
        {
            post.LanguageCode = blogLanguageService.Normalize(post.LanguageCode);
            post.LanguageLabel = blogLanguageService.GetLabel(post.LanguageCode);
        }

        return new BlogAdminIndexViewModel
        {
            Filtro = filtro,
            TotalPosts = await baseQuery.CountAsync(cancellationToken),
            DraftCount = await baseQuery.CountAsync(x => x.Status == BlogPostStatusEnum.Draft, cancellationToken),
            ScheduledCount = await baseQuery.CountAsync(x => x.Status == BlogPostStatusEnum.Scheduled, cancellationToken),
            PublishedCount = await baseQuery.CountAsync(x => x.Status == BlogPostStatusEnum.Published, cancellationToken),
            ArchivedCount = await baseQuery.CountAsync(x => x.Status == BlogPostStatusEnum.Archived, cancellationToken),
            Posts = posts,
            Categories = await blogLookupService.ListCategoriesForFilterAsync(cancellationToken),
            Authors = await blogLookupService.ListAuthorsAsync(cancellationToken)
        };
    }

    public async Task<BlogPostFormViewModel> ObterFormCriacaoAsync(
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        var categorias = await blogLookupService.ListCategoriesForFormAsync(null, cancellationToken);
        var autores = await blogLookupService.ListAuthorsAsync(cancellationToken);

        return new BlogPostFormViewModel
        {
            LanguageCode = blogLanguageService.DefaultLanguageCode,
            LanguageLabel = blogLanguageService.GetLabel(blogLanguageService.DefaultLanguageCode),
            AuthorUserId = autores.Any(x => x.Id == usuarioAtualId) ? usuarioAtualId : autores.FirstOrDefault()?.Id,
            CategoryOptions = categorias,
            AuthorOptions = autores,
            TagSuggestions = await blogLookupService.ListTagSuggestionsAsync(cancellationToken)
        };
    }

    public async Task<BlogPostFormViewModel?> ObterFormEdicaoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await blogWorkflowService.PromoteScheduledPostsAsync(cancellationToken);

        var post = await dbContext.BlogPosts
            .AsNoTracking()
            .Include(x => x.PostTags)
            .ThenInclude(x => x.BlogTag)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);

        if (post is null)
        {
            return null;
        }

        return new BlogPostFormViewModel
        {
            Id = post.Id,
            LanguageCode = blogLanguageService.Normalize(post.LanguageCode),
            LanguageLabel = blogLanguageService.GetLabel(post.LanguageCode),
            TranslationGroupId = post.TranslationGroupId,
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            ContentInput = post.ContentText,
            ContentHtmlInput = post.ContentHtml,
            ContentJsonInput = post.ContentJson,
            CategoryId = post.CategoryId,
            AuthorUserId = post.AuthorUserId,
            IsFeatured = post.IsFeatured,
            IsWeeklyHighlight = post.IsWeeklyHighlight,
            PublicationDateLocal = blogDateTimeService.ConvertUtcToSaoPauloLocal(post.ScheduledAtUtc ?? post.PublishedAtUtc),
            SeoTitle = post.SeoTitle,
            SeoDescription = post.SeoDescription,
            TagsInput = string.Join(", ", post.PostTags
                .OrderBy(x => x.BlogTag.Name)
                .Select(x => x.BlogTag.Name)),
            CurrentCoverImageUrl = post.CoverImageUrl,
            CurrentStatus = post.Status,
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc,
            PublishedAtUtc = post.PublishedAtUtc,
            CategoryOptions = await blogLookupService.ListCategoriesForFormAsync(post.CategoryId, cancellationToken),
            AuthorOptions = await blogLookupService.ListAuthorsAsync(cancellationToken),
            TagSuggestions = await blogLookupService.ListTagSuggestionsAsync(cancellationToken)
        };
    }

    public async Task<BlogPreviewViewModel?> ObterPreviewAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        await blogWorkflowService.PromoteScheduledPostsAsync(cancellationToken);

        return await dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAtUtc == null)
            .Select(x => new BlogPreviewViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                CoverImageUrl = x.CoverImageUrl,
                CategoryName = x.Category != null ? x.Category.Name : null,
                AuthorName = x.AuthorDisplayName,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc,
                PublishedAtUtc = x.PublishedAtUtc,
                IsFeatured = x.IsFeatured,
                IsWeeklyHighlight = x.IsWeeklyHighlight,
                ContentHtml = x.ContentHtml ?? "<p>Conteúdo ainda não informado.</p>",
                Tags = x.PostTags
                    .OrderBy(t => t.BlogTag.Name)
                    .Select(t => t.BlogTag.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
