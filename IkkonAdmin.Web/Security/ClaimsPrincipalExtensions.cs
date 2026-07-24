using System.Security.Claims;

namespace IkkonAdmin.Web.Security;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal? principal, string permissao)
    {
        return AppPermissionEvaluator.HasPermission(principal, permissao);
    }

    public static bool HasAnyPermission(this ClaimsPrincipal? principal, params string[] permissoes)
    {
        return AppPermissionEvaluator.HasAnyPermission(principal, permissoes);
    }
}
