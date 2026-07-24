using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Services;

public sealed record AreaAlunoPortalContexto(
    int AlunoId,
    IReadOnlyCollection<int> TurmaIds,
    string? FotoPerfilUrl);

public sealed class AreaAlunoPerfilBase
{
    public int AlunoId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Celular { get; set; }
    public StatusAlunoEnum Status { get; set; }
    public string? TurmaPrincipal { get; set; }
    public DateOnly DataEntrada { get; set; }
}

public sealed record AreaAlunoResumoFinanceiro(
    decimal TotalEmAberto,
    int MensalidadesAtrasadas);

public sealed record AreaAlunoResumoFrequencia(
    int Total,
    int Presencas,
    int FaltasNaoJustificadas,
    decimal PercentualPresenca);
