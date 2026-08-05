using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class BlogService(
    ApplicationDbContext dbContext,
    IBlogMediaService blogMediaService,
    IBlogWorkflowService blogWorkflowService,
    IBlogLookupService blogLookupService,
    IBlogLanguageService blogLanguageService,
    IBlogTextService blogTextService,
    IBlogSlugService blogSlugService,
    IBlogTagService blogTagService,
    IBlogVersionService blogVersionService) : IBlogService
{
    public async Task<OperationResult<int>> CriarAsync(
        BlogPostFormViewModel model,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        var author = await blogLookupService.GetValidAuthorAsync(model.AuthorUserId, cancellationToken);
        var validacao = await blogWorkflowService.ValidateAsync(
            model,
            author,
            currentCoverUrl: null,
            currentStatus: BlogPostStatusEnum.Draft,
            currentPublishedAtUtc: null,
            cancellationToken);

        if (!validacao.Success)
        {
            return OperationResult<int>.Fail(validacao.Message);
        }

        string? coverImageUrl = null;
        if (model.CoverImage is not null)
        {
            var saveResult = await blogMediaService.SaveCoverImageAsync(model.CoverImage, null, cancellationToken);
            if (!saveResult.Success)
            {
                return OperationResult<int>.Fail(saveResult.Message);
            }

            coverImageUrl = saveResult.PublicUrl;
        }

        var now = DateTime.UtcNow;
        var summary = blogTextService.CleanOptional(model.Summary);
        var contentText = blogTextService.GetContentText(model);
        var contentHtml = blogTextService.GenerateSafeContentHtml(model);
        var contentJson = blogTextService.CleanOptional(model.ContentJsonInput);
        var slug = await blogSlugService.EnsureUniqueAsync(blogTextService.GenerateSlug(model.Slug, model.Title), null, cancellationToken);
        var languageCode = blogLanguageService.Normalize(model.LanguageCode);

        var post = new BlogPost
        {
            Title = model.Title.Trim(),
            Slug = slug,
            Summary = summary,
            ContentHtml = contentHtml,
            ContentJson = contentJson,
            ContentText = contentText,
            CoverImageUrl = coverImageUrl,
            AuthorUserId = author?.Id ?? usuarioAtualId,
            AuthorDisplayName = author?.NomeExibicao ?? author?.Login ?? "Equipe Ikkon",
            CategoryId = model.CategoryId,
            Status = validacao.Status,
            LanguageCode = languageCode,
            IsFeatured = model.IsFeatured || model.IsWeeklyHighlight,
            IsWeeklyHighlight = model.IsWeeklyHighlight,
            PublishedAtUtc = validacao.PublishedAtUtc,
            ScheduledAtUtc = validacao.ScheduledAtUtc,
            ArchivedAtUtc = validacao.ArchivedAtUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            SeoTitle = blogTextService.CleanOptional(model.SeoTitle),
            SeoDescription = blogTextService.CleanOptional(model.SeoDescription) ?? summary,
            ReadingTimeMinutes = blogTextService.CalculateReadingTime(contentText)
        };

        await dbContext.BlogPosts.AddAsync(post, cancellationToken);
        await blogTagService.SyncTagsAsync(post, model.TagsInput, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(
            post.Id,
            validacao.Status == BlogPostStatusEnum.Scheduled ? "Post criado e agendado com sucesso." :
            validacao.Status == BlogPostStatusEnum.Published ? "Post criado e publicado com sucesso." :
            validacao.Status == BlogPostStatusEnum.Archived ? "Post criado e arquivado com sucesso." :
            "Rascunho criado com sucesso.");
    }

    public Task<OperationResult<int>> CriarVersaoAsync(
        int id,
        string languageCode,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        return blogVersionService.CriarVersaoAsync(id, languageCode, usuarioAtualId, cancellationToken);
    }

    public async Task<OperationResult<int>> AtualizarAsync(
        int id,
        BlogPostFormViewModel model,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        await blogWorkflowService.PromoteScheduledPostsAsync(cancellationToken);

        var post = await dbContext.BlogPosts
            .Include(x => x.PostTags)
            .ThenInclude(x => x.BlogTag)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);

        if (post is null)
        {
            return OperationResult<int>.NotFound("Post não encontrado.");
        }

        var author = await blogLookupService.GetValidAuthorAsync(model.AuthorUserId, cancellationToken);
        var validacao = await blogWorkflowService.ValidateAsync(
            model,
            author,
            post.CoverImageUrl,
            post.Status,
            post.PublishedAtUtc,
            cancellationToken);

        if (!validacao.Success)
        {
            return OperationResult<int>.Fail(validacao.Message);
        }

        var coverImageUrl = post.CoverImageUrl;

        if (model.RemoveCoverImage && model.CoverImage is null)
        {
            await blogMediaService.RemoveCoverImageAsync(post.CoverImageUrl, cancellationToken);
            coverImageUrl = null;
        }

        if (model.CoverImage is not null)
        {
            var saveResult = await blogMediaService.SaveCoverImageAsync(model.CoverImage, post.CoverImageUrl, cancellationToken);
            if (!saveResult.Success)
            {
                return OperationResult<int>.Fail(saveResult.Message);
            }

            coverImageUrl = saveResult.PublicUrl;
        }

        var summary = blogTextService.CleanOptional(model.Summary);
        var contentText = blogTextService.GetContentText(model);
        var contentHtml = blogTextService.GenerateSafeContentHtml(model);
        var contentJson = blogTextService.CleanOptional(model.ContentJsonInput);
        var slug = await blogSlugService.EnsureUniqueAsync(blogTextService.GenerateSlug(model.Slug, model.Title), id, cancellationToken);

        post.Title = model.Title.Trim();
        post.Slug = slug;
        post.Summary = summary;
        post.ContentHtml = contentHtml;
        post.ContentJson = contentJson;
        post.ContentText = contentText;
        post.CoverImageUrl = coverImageUrl;
        post.AuthorUserId = author?.Id ?? usuarioAtualId;
        post.AuthorDisplayName = author?.NomeExibicao ?? author?.Login ?? post.AuthorDisplayName;
        post.CategoryId = model.CategoryId;
        post.Status = validacao.Status;
        post.LanguageCode = blogLanguageService.Normalize(post.LanguageCode);
        post.IsFeatured = model.IsFeatured || model.IsWeeklyHighlight;
        post.IsWeeklyHighlight = model.IsWeeklyHighlight;
        post.PublishedAtUtc = validacao.PublishedAtUtc;
        post.ScheduledAtUtc = validacao.ScheduledAtUtc;
        post.ArchivedAtUtc = validacao.ArchivedAtUtc;
        post.UpdatedAtUtc = DateTime.UtcNow;
        post.SeoTitle = blogTextService.CleanOptional(model.SeoTitle);
        post.SeoDescription = blogTextService.CleanOptional(model.SeoDescription) ?? summary;
        post.ReadingTimeMinutes = blogTextService.CalculateReadingTime(contentText);

        await blogTagService.SyncTagsAsync(post, model.TagsInput, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(
            post.Id,
            validacao.Status == BlogPostStatusEnum.Scheduled ? "Post atualizado e agendado com sucesso." :
            validacao.Status == BlogPostStatusEnum.Published ? "Post atualizado e publicado com sucesso." :
            validacao.Status == BlogPostStatusEnum.Archived ? "Post arquivado com sucesso." :
            "Rascunho atualizado com sucesso.");
    }

    public async Task<OperationResult<int>> ExcluirAsync(
        int id,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        var post = await dbContext.BlogPosts.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);
        if (post is null)
        {
            return OperationResult<int>.NotFound("Post não encontrado.");
        }

        post.DeletedAtUtc = DateTime.UtcNow;
        post.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(post.Id, "Post excluído com sucesso.");
    }

    public Task<OperationResult<int>> ExcluirVersaoAsync(
        int id,
        int versionId,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        return blogVersionService.ExcluirVersaoAsync(id, versionId, usuarioAtualId, cancellationToken);
    }
}
