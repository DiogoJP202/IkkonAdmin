namespace IkkonAdmin.Web.Services;

public interface IAulaRecurrenceGenerator
{
    Task<AulaGenerationSummary> GenerateAsync(
        DateOnly? startDate = null,
        int? horizonWeeks = null,
        CancellationToken cancellationToken = default);
}

public sealed record AulaGenerationSummary(
    int SchedulesEvaluated,
    int OccurrencesEvaluated,
    int Created,
    int AlreadyExisting,
    int WithoutInstructor,
    DateOnly StartDate,
    DateOnly EndDateExclusive)
{
    public string ToUserMessage() =>
        $"Geração concluída: {Created} aula(s) criada(s), {AlreadyExisting} já existente(s) e {WithoutInstructor} sem instrutor principal.";
}
