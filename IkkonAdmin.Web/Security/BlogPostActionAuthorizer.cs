using System.Security.Claims;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Security;

public sealed class BlogPostActionAuthorizer : IBlogPostActionAuthorizer
{
    public bool CanUploadContentImage(ClaimsPrincipal? principal)
    {
        return AppPermissionEvaluator.HasAnyPermission(
            principal,
            [AppPermissions.BlogCreate, AppPermissions.BlogEdit]);
    }

    public bool CanSubmit(ClaimsPrincipal? principal, BlogPostFormViewModel model)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if ((model.IsFeatured || model.IsWeeklyHighlight) &&
            !AppPermissionEvaluator.HasPermission(principal, AppPermissions.BlogFeature))
        {
            return false;
        }

        var action = (model.SubmissionAction ?? "Draft").Trim().ToLowerInvariant();
        return action switch
        {
            "publish" => AppPermissionEvaluator.HasPermission(principal, AppPermissions.BlogPublish),
            "schedule" => AppPermissionEvaluator.HasPermission(principal, AppPermissions.BlogPublish),
            "archive" => AppPermissionEvaluator.HasPermission(principal, AppPermissions.BlogArchive),
            _ => true
        };
    }
}
