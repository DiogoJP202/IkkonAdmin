using System.Security.Claims;

namespace IkkonAdmin.Web.Infrastructure.Security;

public sealed class HttpCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public int? UserId
    {
        get
        {
            var value = FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var userId) ? userId : null;
        }
    }

    public string? UserName => User?.Identity?.Name;

    public string? RemoteIpAddress => httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public bool IsInRole(string role)
    {
        return User?.IsInRole(role) == true;
    }

    public bool HasClaim(string type, string value)
    {
        return User?.HasClaim(type, value) == true;
    }

    public string? FindFirstValue(string claimType)
    {
        return User?.FindFirstValue(claimType);
    }
}
