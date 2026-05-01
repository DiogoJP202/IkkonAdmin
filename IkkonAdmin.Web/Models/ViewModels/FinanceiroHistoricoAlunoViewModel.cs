using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class FinanceiroHistoricoAlunoViewModel
{
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string? Turma { get; set; }
    public decimal TotalPago { get; set; }
    public decimal TotalEmAberto { get; set; }

    public IReadOnlyCollection<FinanceiroMensalidadeItemViewModel> Mensalidades { get; set; } = [];
    public IReadOnlyCollection<FinanceiroPagamentoItemViewModel> Pagamentos { get; set; } = [];
}

public class FinanceiroPagamentoItemViewModel
{
    public int PagamentoId { get; set; }
    public int MensalidadeId { get; set; }
    public DateOnly? Competencia { get; set; }
    public DateTime DataPagamento { get; set; }
    public decimal ValorPago { get; set; }
    public FormaPagamentoEnum FormaPagamento { get; set; }
    public string? Observacoes { get; set; }
}
