namespace IkkonAdmin.Web.Models.Entities;

public class GoogleAgendaConexao
{
    public int Id { get; set; }
    public string? ContaEmail { get; set; }
    public string RefreshTokenProtegido { get; set; } = string.Empty;
    public string Escopos { get; set; } = string.Empty;
    public bool Ativa { get; set; } = true;
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEmUtc { get; set; }
    public int? ConectadoPorUsuarioId { get; set; }
    public UsuarioSistema? ConectadoPorUsuario { get; set; }
}
