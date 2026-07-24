using System.Security.Claims;

namespace IkkonAdmin.Web.Infrastructure.Security;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    int? UserId { get; }
    string? UserName { get; }
    string? RemoteIpAddress { get; }
    bool IsInRole(string role);
    bool HasClaim(string type, string value);
    string? FindFirstValue(string claimType);
}
