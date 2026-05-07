using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class InventarioItem
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CodigoInterno { get; set; }
    public InventarioCategoriaEnum Categoria { get; set; } = InventarioCategoriaEnum.Taiko;
    public string? Tipo { get; set; }
    public string? Descricao { get; set; }
    public int Quantidade { get; set; } = 1;
    public InventarioStatusEnum Status { get; set; } = InventarioStatusEnum.Disponivel;
    public InventarioEstadoConservacaoEnum EstadoConservacao { get; set; } = InventarioEstadoConservacaoEnum.Bom;
    public string? Localizacao { get; set; }
    public bool DisponivelParaAula { get; set; } = true;
    public bool DisponivelParaEvento { get; set; } = true;
    public DateOnly? DataAquisicao { get; set; }
    public decimal? ValorEstimado { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEmUtc { get; set; }
    public int? CriadoPorUsuarioId { get; set; }
    public UsuarioSistema? CriadoPorUsuario { get; set; }
    public int? AtualizadoPorUsuarioId { get; set; }
    public UsuarioSistema? AtualizadoPorUsuario { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<InventarioMovimentacao> Movimentacoes { get; set; } = new List<InventarioMovimentacao>();
}
