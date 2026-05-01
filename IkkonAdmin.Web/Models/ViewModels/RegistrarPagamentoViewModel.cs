using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class RegistrarPagamentoViewModel
{
    [Required]
    public int MensalidadeId { get; set; }

    [Required]
    public int AlunoId { get; set; }

    public string? AlunoNome { get; set; }
    public DateOnly? Competencia { get; set; }
    public DateOnly? DataVencimento { get; set; }
    public decimal? ValorMensalidadeAtual { get; set; }
    public StatusMensalidadeEnum? StatusMensalidadeAtual { get; set; }

    [Required]
    public DateTime DataPagamento { get; set; } = DateTime.Now;

    [Range(0.01, 999999)]
    public decimal ValorPago { get; set; }

    [Required]
    public FormaPagamentoEnum FormaPagamento { get; set; } = FormaPagamentoEnum.Pix;

    public string? Observacoes { get; set; }
    public string? ReturnUrl { get; set; }
}
