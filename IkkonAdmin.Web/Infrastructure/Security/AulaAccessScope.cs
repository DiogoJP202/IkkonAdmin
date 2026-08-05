namespace IkkonAdmin.Web.Infrastructure.Security;

public readonly record struct AulaAccessScope(int? UserId, bool HasGlobalAccess)
{
    public static AulaAccessScope Global(int? userId) => new(userId, true);
    public static AulaAccessScope Restricted(int? userId) => new(userId, false);
}
