using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Services;

namespace IkkonAdmin.Web.Infrastructure.Maintenance;

public sealed class StudentAutomationHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<StudentAutomationHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = await CalculateDelayAsync(stoppingToken);
                await Task.Delay(delay, stoppingToken);
                await ExecuteAutomationsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Falha no ciclo de automações da Área do Aluno.");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task<TimeSpan> CalculateDelayAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var configurationProvider = scope.ServiceProvider.GetRequiredService<IConfiguracaoSistemaProvider>();
        var configuration = await configurationProvider.ObterOuCriarAsync(cancellationToken);

        if (!configuration.GerarAulasAutomaticamente && !configuration.AvaliarConquistasAutomaticamente)
        {
            return TimeSpan.FromMinutes(15);
        }

        var utcNow = DateTime.UtcNow;
        var localNow = SaoPauloTime.FromUtc(utcNow);
        var nextLocal = localNow.Date.Add(configuration.HorarioAutomacoesAreaAluno.ToTimeSpan());
        if (nextLocal <= localNow)
        {
            nextLocal = nextLocal.AddDays(1);
        }

        var nextUtc = TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(nextLocal, DateTimeKind.Unspecified),
            SaoPauloTime.TimeZone);
        return nextUtc - utcNow;
    }

    private async Task ExecuteAutomationsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var configurationProvider = scope.ServiceProvider.GetRequiredService<IConfiguracaoSistemaProvider>();
        var configuration = await configurationProvider.ObterOuCriarAsync(cancellationToken);

        if (configuration.GerarAulasAutomaticamente)
        {
            var generator = scope.ServiceProvider.GetRequiredService<IAulaRecurrenceGenerator>();
            var summary = await generator.GenerateAsync(
                horizonWeeks: configuration.HorizonteGeracaoAulasSemanas,
                cancellationToken: cancellationToken);
            logger.LogInformation(
                "Geração automática de aulas concluída: {Created} criadas e {Existing} existentes.",
                summary.Created,
                summary.AlreadyExisting);
        }

        if (configuration.AvaliarConquistasAutomaticamente)
        {
            var evaluator = scope.ServiceProvider.GetRequiredService<IInsigniaRuleEvaluator>();
            var summary = await evaluator.EvaluateAsync(cancellationToken: cancellationToken);
            logger.LogInformation(
                "Avaliação automática de conquistas concluída: {Granted} concedidas e {Invalid} regras inválidas.",
                summary.AchievementsGranted,
                summary.InvalidRules.Count);
        }
    }
}
