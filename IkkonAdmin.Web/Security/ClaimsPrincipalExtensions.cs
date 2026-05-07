using System.Security.Claims;

namespace IkkonAdmin.Web.Security;

public static class ClaimsPrincipalExtensions
{
    public static bool HasPermission(this ClaimsPrincipal? principal, string permissao)
    {
        return principal is not null &&
            (principal.IsInRole(AppRoles.Admin) || principal.HasClaim(AppClaimTypes.Permissao, permissao));
    }

    public static bool HasAnyPermission(this ClaimsPrincipal? principal, params string[] permissoes)
    {
        if (principal is null || permissoes.Length == 0)
        {
            return false;
        }

        return principal.IsInRole(AppRoles.Admin) ||
            permissoes.Any(permissao => principal.HasClaim(AppClaimTypes.Permissao, permissao));
    }
}
