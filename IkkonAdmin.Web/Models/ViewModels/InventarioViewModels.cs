using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class InventarioFiltroViewModel
{
    public string? Busca { get; set; }
    public InventarioCategoriaEnum? Categoria { get; set; }
    public string? Tipo { get; set; }
    public InventarioStatusEnum? Status { get; set; }
    public InventarioEstadoConservacaoEnum? EstadoConservacao { get; set; }
    public string? Localizacao { get; set; }
    public int PaginaAtual { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
}

public class InventarioIndexViewModel
{
    public InventarioFiltroViewModel Filtro { get; set; } = new();
    public int TotalRegistros { get; set; }
    public int TotalItens { get; set; }
    public int ItensDisponiveis { get; set; }
    public int ItensManutencao { get; set; }
    public int ItensIndisponiveis { get; set; }
    public int TotalPaginas => TamanhoPagina <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalRegistros / (decimal)TamanhoPagina));
    public int PaginaAtual => Filtro.PaginaAtual;
    public int TamanhoPagina => Filtro.TamanhoPagina;
    public List<InventarioItemViewModel> Itens { get; set; } = new();
    public List<string> TiposDisponiveis { get; set; } = new();
    public List<string> LocalizacoesDisponiveis { get; set; } = new();
}

public class InventarioItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? CodigoInterno { get; set; }
    public InventarioCategoriaEnum Categoria { get; set; }
    public string? Tipo { get; set; }
    public int Quantidade { get; set; }
    public InventarioStatusEnum Status { get; set; }
    public InventarioEstadoConservacaoEnum EstadoConservacao { get; set; }
    public string? Localizacao { get; set; }
    public bool DisponivelParaAula { get; set; }
    public bool DisponivelParaEvento { get; set; }
    public bool Ativo { get; set; }
}

public class InventarioDetalhesViewModel : InventarioItemViewModel
{
    public string? Descricao { get; set; }
    public DateOnly? DataAquisicao { get; set; }
    public decimal? ValorEstimado { get; set; }
    public string? Observacoes { get; set; }
    public DateTime CriadoEmUtc { get; set; }
    public DateTime? AtualizadoEmUtc { get; set; }
    public string? CriadoPorNome { get; set; }
    public string? AtualizadoPorNome { get; set; }
    public List<InventarioMovimentacaoResumoViewModel> MovimentacoesRecentes { get; set; } = new();
}

public class InventarioMovimentacaoResumoViewModel
{
    public int Id { get; set; }
    public InventarioTipoMovimentacaoEnum TipoMovimentacao { get; set; }
    public int Quantidade { get; set; }
    public DateTime DataInicioUtc { get; set; }
    public DateTime? DataFimUtc { get; set; }
    public string? ResponsavelNome { get; set; }
    public string? GoogleEventId { get; set; }
    public string? Observacoes { get; set; }
}

public class InventarioFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Informe o nome do item.")]
    [StringLength(150, ErrorMessage = "Nome deve ter no máximo 150 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Código interno")]
    [StringLength(60, ErrorMessage = "Código interno deve ter no máximo 60 caracteres.")]
    public string? CodigoInterno { get; set; }

    [Display(Name = "Categoria")]
    [Required(ErrorMessage = "Selecione a categoria.")]
    public InventarioCategoriaEnum Categoria { get; set; } = InventarioCategoriaEnum.Taiko;

    [Display(Name = "Tipo")]
    [StringLength(80, ErrorMessage = "Tipo deve ter no máximo 80 caracteres.")]
    public string? Tipo { get; set; }

    [Display(Name = "Descrição")]
    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres.")]
    public string? Descricao { get; set; }

    [Display(Name = "Quantidade")]
    [Range(0, 9999, ErrorMessage = "Quantidade deve ser maior ou igual a zero.")]
    public int Quantidade { get; set; } = 1;

    [Display(Name = "Status")]
    [Required(ErrorMessage = "Selecione o status.")]
    public InventarioStatusEnum Status { get; set; } = InventarioStatusEnum.Disponivel;

    [Display(Name = "Estado de conservação")]
    [Required(ErrorMessage = "Selecione o estado de conservação.")]
    public InventarioEstadoConservacaoEnum EstadoConservacao { get; set; } = InventarioEstadoConservacaoEnum.Bom;

    [Display(Name = "Localização")]
    [StringLength(120, ErrorMessage = "Localização deve ter no máximo 120 caracteres.")]
    public string? Localizacao { get; set; }

    [Display(Name = "Disponível para aulas")]
    public bool DisponivelParaAula { get; set; } = true;

    [Display(Name = "Disponível para eventos")]
    public bool DisponivelParaEvento { get; set; } = true;

    [Display(Name = "Data de aquisição")]
    [DataType(DataType.Date)]
    public DateOnly? DataAquisicao { get; set; }

    [Display(Name = "Valor estimado")]
    [Range(0, 9999999, ErrorMessage = "Valor estimado não pode ser negativo.")]
    public decimal? ValorEstimado { get; set; }

    [Display(Name = "Observações")]
    [StringLength(1000, ErrorMessage = "Observações devem ter no máximo 1000 caracteres.")]
    public string? Observacoes { get; set; }

    public List<string> TiposSugeridos { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataAquisicao.HasValue && DataAquisicao.Value > DateOnly.FromDateTime(DateTime.Today))
        {
            yield return new ValidationResult(
                "Data de aquisição não pode ser futura.",
                new[] { nameof(DataAquisicao) });
        }

        if (Categoria == InventarioCategoriaEnum.Taiko && string.IsNullOrWhiteSpace(Tipo))
        {
            yield return new ValidationResult(
                "Informe o tipo do taiko.",
                new[] { nameof(Tipo) });
        }
    }
}

public sealed record InventarioOperationResult(bool Success, string Message, int? EntityId = null)
{
    public static InventarioOperationResult Ok(string message, int? entityId = null) => new(true, message, entityId);
    public static InventarioOperationResult Fail(string message) => new(false, message);
}
