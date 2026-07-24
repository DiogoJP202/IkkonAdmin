namespace IkkonAdmin.Web.Security;

public sealed record PermissionPolicyDefinition(
    string PolicyName,
    PermissionPolicyScope Scope,
    IReadOnlyCollection<string> Permissions);
