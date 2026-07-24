using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class BlogWorkflowService(
    ApplicationDbContext dbContext,
    IBlogLookupService blogLookupService,
    IBlogSlugService blogSlugService,
    IBlogTextService blogTextService,
    IBlogDateTimeService blogDateTimeService) : IBlogWorkflowService
{
    public async Task PromoteScheduledPostsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var scheduledPosts = await dbContext.BlogPosts
            .Where(x => x.DeletedAtUtc == null &&
                        x.Status == BlogPostStatusEnum.Scheduled &&
                        x.ScheduledAtUtc.HasValue &&
                        x.ScheduledAtUtc <= now)
            .ToListAsync(cancellationToken);

        if (scheduledPosts.Count == 0)
        {
            return;
        }

        foreach (var post in scheduledPosts)
        {
            post.Status = BlogPostStatusEnum.Published;
            post.PublishedAtUtc = post.ScheduledAtUtc ?? now;
            post.ScheduledAtUtc = null;
            post.ArchivedAtUtc = null;
            post.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BlogWorkflowValidation> ValidateAsync(
        BlogPostFormViewModel model,
        UsuarioSistema? author,
        string? currentCoverUrl,
        BlogPostStatusEnum currentStatus,
        DateTime? currentPublishedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var acao = (model.SubmissionAction ?? "Draft").Trim().ToLowerInvariant();
        var publicationDateUtc = model.PublicationDateLocal.HasValue
            ? blogDateTimeService.ConvertSaoPauloLocalToUtc(model.PublicationDateLocal.Value)
            : (DateTime?)null;
        var now = DateTime.UtcNow;
        var summary = blogTextService.CleanOptional(model.Summary);
        var contentText = blogTextService.GetContentText(model);
        var hasContent = !string.IsNullOrWhiteSpace(contentText) ||
                         blogTextService.HasHtmlMedia(blogTextService.SanitizeHtmlForValidation(model.ContentHtmlInput));
        var hasCover = model.CoverImage is not null ||
                       (!model.RemoveCoverImage && !string.IsNullOrWhiteSpace(currentCoverUrl));
        var slugBase = blogTextService.GenerateSlug(model.Slug, model.Title);

        if (!await blogLookupService.IsCategoryValidAsync(model.CategoryId, cancellationToken))
        {
            return BlogWorkflowValidation.Fail("Selecione uma categoria ativa.");
        }

        if (!string.IsNullOrWhiteSpace(model.Slug) &&
            await blogSlugService.ExistsAsync(slugBase, model.Id, cancellationToken))
        {
            return BlogWorkflowValidation.Fail("Já existe um post com esse slug.");
        }

        switch (acao)
        {
            case "publish":
                if (publicationDateUtc.HasValue && publicationDateUtc.Value > now)
                {
                    return BlogWorkflowValidation.Fail("Para data futura, use a ação Agendar.");
                }

                var pendenciasPublicacao = ListPublicationMissingFields(summary, hasContent, author, hasCover, model.CategoryId);
                if (pendenciasPublicacao.Count > 0)
                {
                    return BlogWorkflowValidation.Fail($"Para publicar, informe {FormatMissingFields(pendenciasPublicacao)}.");
                }

                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Published,
                    publicationDateUtc ?? now,
                    null,
                    null);

            case "schedule":
                if (!publicationDateUtc.HasValue || publicationDateUtc.Value <= now)
                {
                    return BlogWorkflowValidation.Fail("Informe uma data futura para agendar a publicação.");
                }

                var pendenciasAgendamento = ListPublicationMissingFields(summary, hasContent, author, hasCover, model.CategoryId);
                if (pendenciasAgendamento.Count > 0)
                {
                    return BlogWorkflowValidation.Fail($"Para agendar, informe {FormatMissingFields(pendenciasAgendamento)}.");
                }

                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Scheduled,
                    null,
                    publicationDateUtc,
                    null);

            case "archive":
                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Archived,
                    currentStatus == BlogPostStatusEnum.Published ? currentPublishedAtUtc : null,
                    null,
                    now);

            default:
                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Draft,
                    null,
                    null,
                    null);
        }
    }

    private static List<string> ListPublicationMissingFields(
        string? summary,
        bool hasContent,
        UsuarioSistema? author,
        bool hasCover,
        int? categoryId)
    {
        var missingFields = new List<string>();

        if (string.IsNullOrWhiteSpace(summary))
        {
            missingFields.Add("resumo");
        }

        if (!hasContent)
        {
            missingFields.Add("conteúdo");
        }

        if (!categoryId.HasValue)
        {
            missingFields.Add("categoria");
        }

        if (author is null)
        {
            missingFields.Add("autor");
        }

        if (!hasCover)
        {
            missingFields.Add("imagem de capa");
        }

        return missingFields;
    }

    private static string FormatMissingFields(IReadOnlyList<string> missingFields)
    {
        return missingFields.Count switch
        {
            0 => "os campos obrigatórios",
            1 => missingFields[0],
            2 => $"{missingFields[0]} e {missingFields[1]}",
            _ => $"{string.Join(", ", missingFields.Take(missingFields.Count - 1))} e {missingFields[^1]}"
        };
    }
}
