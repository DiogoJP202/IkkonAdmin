using System.Security.Claims;
using System.Text.Encodings.Web;
using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IkkonAdmin.Tests.Integration;

internal sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "IntegrationTest";
    public const string UserIdHeader = "X-Test-User-Id";
    public const string RolesHeader = "X-Test-Roles";
    public const string PermissionsHeader = "X-Test-Permissions";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserIdHeader, out var userId) ||
            string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, $"integration-user-{userId}")
        };

        AddDelimitedClaims(claims, RolesHeader, ClaimTypes.Role);
        AddDelimitedClaims(claims, PermissionsHeader, AppClaimTypes.Permissao);

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }

    private void AddDelimitedClaims(List<Claim> claims, string headerName, string claimType)
    {
        if (!Request.Headers.TryGetValue(headerName, out var values))
        {
            return;
        }

        foreach (var value in values.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            claims.Add(new Claim(claimType, value));
        }
    }
}
