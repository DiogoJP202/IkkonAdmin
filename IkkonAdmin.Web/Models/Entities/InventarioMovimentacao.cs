using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class InventarioMovimentacao
{
    public int Id { get; set; }
    public int InventarioItemId { get; set; }
    public InventarioItem? InventarioItem { get; set; }
    public string? GoogleEventId { get; set; }
    public InventarioTipoMovimentacaoEnum TipoMovimentacao { get; set; } = InventarioTipoMovimentacaoEnum.Reserva;
    public int Quantidade { get; set; } = 1;
    public DateTime DataInicioUtc { get; set; }
    public DateTime? DataFimUtc { get; set; }
    public int? ResponsavelUsuarioId { get; set; }
    public UsuarioSistema? ResponsavelUsuario { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
}
