namespace IkkonAdmin.Web.Infrastructure.Auditing;

public sealed record AuditLogEntry
{
    public int? UsuarioResponsavelId { get; init; }
    public int? UsuarioAfetadoId { get; init; }
    public required string Acao { get; init; }
    public required string Entidade { get; init; }
    public string? EntidadeId { get; init; }
    public string? Descricao { get; init; }
    public string? DadosAntesJson { get; init; }
    public string? DadosDepoisJson { get; init; }
    public string? EnderecoIp { get; init; }
    public string? CorrelationId { get; init; }
}
