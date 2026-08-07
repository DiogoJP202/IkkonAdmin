using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IkkonAdmin.Web.Infrastructure.Health;

public static class HealthCheckResponseWriter
{
    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            status = report.Status.ToString().ToLowerInvariant(),
            checkedAtUtc = DateTimeOffset.UtcNow,
            durationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => new
                {
                    status = entry.Value.Status.ToString().ToLowerInvariant(),
                    description = entry.Value.Description,
                    durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2)
                })
        }));
    }
}
