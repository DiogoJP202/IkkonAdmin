namespace IkkonAdmin.Web.Models.Entities;

public class AuditoriaLog
{
    public long Id { get; set; }
    public int? UsuarioResponsavelId { get; set; }
    public UsuarioSistema? UsuarioResponsavel { get; set; }

    public int? UsuarioAfetadoId { get; set; }
    public UsuarioSistema? UsuarioAfetado { get; set; }

    public string Acao { get; set; } = string.Empty;
    public string Entidade { get; set; } = string.Empty;
    public string? EntidadeId { get; set; }
    public string? Descricao { get; set; }
    public string? DadosAntesJson { get; set; }
    public string? DadosDepoisJson { get; set; }
    public string? EnderecoIp { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime DataEventoUtc { get; set; } = DateTime.UtcNow;
}
