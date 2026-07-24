namespace IkkonAdmin.Web.Infrastructure.Auditing;

public interface IAuditLogger
{
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
