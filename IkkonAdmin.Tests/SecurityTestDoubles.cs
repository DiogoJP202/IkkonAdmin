using IkkonAdmin.Web.Infrastructure.Auditing;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Security;

namespace IkkonAdmin.Tests;

internal sealed class RecordingAuditLogger : IAuditLogger
{
    public List<AuditLogEntry> Entries { get; } = [];

    public Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Entries.Add(entry);
        return Task.CompletedTask;
    }
}

internal sealed class StubCurrentUserService(
    int? userId = 1,
    bool isAdmin = true,
    IReadOnlyCollection<string>? permissions = null) : ICurrentUserService
{
    private readonly IReadOnlyCollection<string> _permissions = permissions ?? [];

    public bool IsAuthenticated => userId.HasValue;
    public int? UserId => userId;
    public string? UserName => userId.HasValue ? "usuario.teste" : null;
    public string? RemoteIpAddress => "127.0.0.1";
    public string? CorrelationId => "test-correlation-id";
    public bool IsInRole(string role) => isAdmin && role == AppRoles.Admin;
    public bool HasClaim(string type, string value) =>
        type == AppClaimTypes.Permissao && _permissions.Contains(value);
    public string? FindFirstValue(string claimType) => null;
}
