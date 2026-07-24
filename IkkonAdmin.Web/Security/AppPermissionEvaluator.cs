using System.Security.Claims;

namespace IkkonAdmin.Web.Security;

public static class AppPermissionEvaluator
{
    public static bool HasPermission(ClaimsPrincipal? principal, string permission)
    {
        return HasAnyPermission(principal, [permission]);
    }

    public static bool HasAnyPermission(ClaimsPrincipal? principal, IReadOnlyCollection<string> permissions)
    {
        if (principal is null || permissions.Count == 0)
        {
            return false;
        }

        return principal.IsInRole(AppRoles.Admin) ||
               permissions.Any(permission => principal.HasClaim(AppClaimTypes.Permissao, permission));
    }

    public static bool HasFuncionarioPermission(ClaimsPrincipal? principal, IReadOnlyCollection<string> permissions)
    {
        return principal is not null &&
               (principal.IsInRole(AppRoles.Admin) ||
                (principal.IsInRole(AppRoles.Funcionario) && HasAnyPermissionClaim(principal, permissions)));
    }

    public static bool HasAuthenticatedPermission(ClaimsPrincipal? principal, IReadOnlyCollection<string> permissions)
    {
        return principal is not null &&
               (principal.IsInRole(AppRoles.Admin) || HasAnyPermissionClaim(principal, permissions));
    }

    private static bool HasAnyPermissionClaim(ClaimsPrincipal principal, IReadOnlyCollection<string> permissions)
    {
        return permissions.Count > 0 &&
               permissions.Any(permission => principal.HasClaim(AppClaimTypes.Permissao, permission));
    }
}
