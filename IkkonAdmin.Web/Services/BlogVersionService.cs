using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class BlogVersionService(
    ApplicationDbContext dbContext,
    IBlogLanguageService blogLanguageService,
    IBlogTextService blogTextService,
    IBlogSlugService blogSlugService,
    IBlogTagService blogTagService) : IBlogVersionService
{
    public async Task<BlogVersionOverviewViewModel?> ObterVersoesAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var source = await dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAtUtc == null)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.LanguageCode,
                x.TranslationGroupId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            return null;
        }

        var groupId = source.TranslationGroupId ?? source.Id;
        var versions = await dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.DeletedAtUtc == null &&
                        (x.Id == groupId || x.TranslationGroupId == groupId))
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.LanguageCode,
                x.Status,
                x.UpdatedAtUtc,
                x.PublishedAtUtc,
                x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new BlogVersionOverviewViewModel
        {
            SourcePostId = source.Id,
            TranslationGroupId = groupId,
            SourceTitle = source.Title,
            SourceLanguageCode = blogLanguageService.Normalize(source.LanguageCode),
            Versions = blogLanguageService.SupportedLanguages
                .Select(language =>
                {
                    var version = versions
                        .Where(x => string.Equals(blogLanguageService.Normalize(x.LanguageCode), language.Code, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(x => x.Id == source.Id)
                        .ThenBy(x => x.Id)
                        .FirstOrDefault();

                    return new BlogVersionItemViewModel
                    {
                        LanguageCode = language.Code,
                        LanguageLabel = language.Label,
                        NativeLabel = language.NativeLabel,
                        SlugSuffix = language.SlugSuffix,
                        IsDefault = language.IsDefault,
                        IsCurrent = version?.Id == source.Id,
                        PostId = version?.Id,
                        Title = version?.Title,
                        Slug = version?.Slug,
                        Status = version?.Status,
                        UpdatedAtUtc = version?.UpdatedAtUtc ?? version?.CreatedAtUtc,
                        PublishedAtUtc = version?.PublishedAtUtc
                    };
                })
                .ToList()
        };
    }

    public async Task<OperationResult<int>> CriarVersaoAsync(
        int id,
        string languageCode,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        var language = blogLanguageService.GetDefinition(languageCode);
        if (language is null)
        {
            return OperationResult<int>.Fail("Idioma não suportado para versões do blog.");
        }

        var source = await dbContext.BlogPosts
            .Include(x => x.PostTags)
            .ThenInclude(x => x.BlogTag)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);

        if (source is null)
        {
            return OperationResult<int>.NotFound("Post não encontrado.");
        }

        var groupId = source.TranslationGroupId ?? source.Id;
        var versionExists = await dbContext.BlogPosts.AnyAsync(
            x => x.DeletedAtUtc == null &&
                 (x.Id == groupId || x.TranslationGroupId == groupId) &&
                 x.LanguageCode == language.Code,
            cancellationToken);

        if (versionExists)
        {
            return OperationResult<int>.Conflict("Já existe uma versão neste idioma.", nameof(languageCode));
        }

        var now = DateTime.UtcNow;
        var sourceSlug = string.IsNullOrWhiteSpace(source.Slug)
            ? blogTextService.GenerateSlug(null, source.Title)
            : source.Slug;
        var slug = await blogSlugService.EnsureUniqueAsync(
            blogTextService.GenerateSlug($"{sourceSlug}-{language.SlugSuffix}", $"{source.Title}-{language.SlugSuffix}"),
            null,
            cancellationToken);
        var tagsInput = string.Join(", ", source.PostTags
            .OrderBy(x => x.BlogTag.Name)
            .Select(x => x.BlogTag.Name));

        var version = new BlogPost
        {
            Title = source.Title,
            Slug = slug,
            Summary = source.Summary,
            ContentHtml = source.ContentHtml,
            ContentJson = source.ContentJson,
            ContentText = source.ContentText,
            CoverImageUrl = source.CoverImageUrl,
            AuthorUserId = source.AuthorUserId ?? usuarioAtualId,
            AuthorDisplayName = source.AuthorDisplayName,
            CategoryId = source.CategoryId,
            Status = BlogPostStatusEnum.Draft,
            LanguageCode = language.Code,
            TranslationGroupId = groupId,
            IsFeatured = source.IsFeatured,
            IsWeeklyHighlight = source.IsWeeklyHighlight,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            SeoTitle = source.SeoTitle,
            SeoDescription = source.SeoDescription,
            ReadingTimeMinutes = source.ReadingTimeMinutes
        };

        await dbContext.BlogPosts.AddAsync(version, cancellationToken);
        await blogTagService.SyncTagsAsync(version, tagsInput, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(version.Id, "Versão criada como rascunho. Revise a tradução antes de publicar.");
    }

    public async Task<OperationResult<int>> ExcluirVersaoAsync(
        int id,
        int versionId,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        if (id == versionId)
        {
            return OperationResult<int>.Fail("Use a ação principal do post para excluir a versão aberta.");
        }

        var source = await dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAtUtc == null)
            .Select(x => new { x.Id, x.TranslationGroupId })
            .FirstOrDefaultAsync(cancellationToken);

        var version = await dbContext.BlogPosts
            .FirstOrDefaultAsync(x => x.Id == versionId && x.DeletedAtUtc == null, cancellationToken);

        if (source is null || version is null)
        {
            return OperationResult<int>.NotFound("Versão não encontrada.");
        }

        var sourceGroupId = source.TranslationGroupId ?? source.Id;
        var versionGroupId = version.TranslationGroupId ?? version.Id;
        if (sourceGroupId != versionGroupId)
        {
            return OperationResult<int>.Fail("Esta versão não pertence ao post atual.");
        }

        var now = DateTime.UtcNow;
        version.DeletedAtUtc = now;
        version.UpdatedAtUtc = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(version.Id, "Versão excluída com sucesso.");
    }
}
