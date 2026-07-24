using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogWorkflowService
{
    Task PromoteScheduledPostsAsync(CancellationToken cancellationToken = default);
    Task<BlogWorkflowValidation> ValidateAsync(
        BlogPostFormViewModel model,
        UsuarioSistema? author,
        string? currentCoverUrl,
        BlogPostStatusEnum currentStatus,
        DateTime? currentPublishedAtUtc,
        CancellationToken cancellationToken = default);
}
