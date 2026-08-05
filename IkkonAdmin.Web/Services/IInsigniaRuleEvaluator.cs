using IkkonAdmin.Web.Infrastructure.Operations;

namespace IkkonAdmin.Web.Services;

public interface IInsigniaRuleEvaluator
{
    OperationResult ValidateRule(string? rule);

    Task<InsigniaProcessingSummary> EvaluateAsync(
        IReadOnlyCollection<int>? studentIds = null,
        CancellationToken cancellationToken = default);
}

public sealed record InsigniaProcessingSummary(
    int RulesEvaluated,
    int AchievementsGranted,
    int AlreadyExisting,
    IReadOnlyCollection<string> InvalidRules)
{
    public string ToUserMessage() =>
        $"Avaliação concluída: {AchievementsGranted} conquista(s) concedida(s), {AlreadyExisting} já existente(s) e {InvalidRules.Count} regra(s) inválida(s).";
}
