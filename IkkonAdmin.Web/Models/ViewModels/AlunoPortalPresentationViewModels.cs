namespace IkkonAdmin.Web.Models.ViewModels;

public sealed record AlunoPageHeaderViewModel(
    string Eyebrow,
    string Title,
    string Description,
    string? Meta = null);

public sealed record AlunoMetricCardViewModel(
    string Label,
    string Value,
    string Tone = "neutral",
    string? Hint = null);

public sealed record AlunoStatusBadgeViewModel(
    string Text,
    string Tone = "neutral");
