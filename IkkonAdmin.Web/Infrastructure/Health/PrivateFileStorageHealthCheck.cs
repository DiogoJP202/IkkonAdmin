using IkkonAdmin.Web.Infrastructure.Files;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IkkonAdmin.Web.Infrastructure.Health;

public sealed class PrivateFileStorageHealthCheck(
    IPrivateFileStorageHealthProbe healthProbe) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await healthProbe.CheckAvailabilityAsync(cancellationToken);
            return HealthCheckResult.Healthy("Storage privado disponível.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy("Storage privado indisponível.", exception);
        }
    }
}
