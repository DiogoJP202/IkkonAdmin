using System.Security.Claims;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Security;

public interface IBlogPostActionAuthorizer
{
    bool CanUploadContentImage(ClaimsPrincipal? principal);
    bool CanSubmit(ClaimsPrincipal? principal, BlogPostFormViewModel model);
}
