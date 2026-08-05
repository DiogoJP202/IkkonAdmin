using IkkonAdmin.Web.Services;
using Microsoft.Extensions.Options;

namespace IkkonAdmin.Web.Infrastructure.Maintenance;

public sealed class OperationalMaintenanceHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<OperationalMaintenanceOptions> options,
    ILogger<OperationalMaintenanceHostedService> logger) : BackgroundService
{
    private static readonly TimeZoneInfo SaoPauloTimeZone = ResolveSaoPauloTimeZone();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Manutenção operacional automática desabilitada.");
            return;
        }

        ValidateSchedule(settings);

        if (settings.RunOnStartup)
        {
            await ExecuteMaintenanceSafelyAsync(stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun(settings, DateTimeOffset.UtcNow);
            await Task.Delay(delay, stoppingToken);
            await ExecuteMaintenanceSafelyAsync(stoppingToken);
        }
    }

    private async Task ExecuteMaintenanceSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var workflowService = scope.ServiceProvider.GetRequiredService<IBlogWorkflowService>();
            var financeiroService = scope.ServiceProvider.GetRequiredService<IFinanceiroService>();

            await workflowService.PromoteScheduledPostsAsync(cancellationToken);
            var mensalidadesAtualizadas = await financeiroService.AtualizarAtrasosAsync(cancellationToken);

            logger.LogInformation(
                "Manutenção operacional concluída. Mensalidades marcadas como atrasadas: {Quantidade}.",
                mensalidadesAtualizadas);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Encerramento normal da aplicação.
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Falha na execução da manutenção operacional.");
        }
    }

    internal static TimeSpan CalculateDelayUntilNextRun(
        OperationalMaintenanceOptions settings,
        DateTimeOffset utcNow)
    {
        var localNow = TimeZoneInfo.ConvertTime(utcNow, SaoPauloTimeZone);
        var nextLocal = new DateTimeOffset(
            localNow.Year,
            localNow.Month,
            localNow.Day,
            settings.HourLocal,
            settings.MinuteLocal,
            0,
            localNow.Offset);

        if (nextLocal <= localNow)
        {
            nextLocal = nextLocal.AddDays(1);
        }

        var nextUtc = TimeZoneInfo.ConvertTime(nextLocal, TimeZoneInfo.Utc);
        return nextUtc - utcNow;
    }

    private static void ValidateSchedule(OperationalMaintenanceOptions settings)
    {
        if (settings.HourLocal is < 0 or > 23)
        {
            throw new InvalidOperationException("OperationalMaintenance:HourLocal deve estar entre 0 e 23.");
        }

        if (settings.MinuteLocal is < 0 or > 59)
        {
            throw new InvalidOperationException("OperationalMaintenance:MinuteLocal deve estar entre 0 e 59.");
        }
    }

    private static TimeZoneInfo ResolveSaoPauloTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }
}
