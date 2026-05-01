using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class Pagamento
{
    public int Id { get; set; }

    public int MensalidadeId { get; set; }
    public Mensalidade? Mensalidade { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public DateTime DataPagamento { get; set; } = DateTime.Now;
    public decimal ValorPago { get; set; }

    public FormaPagamentoEnum FormaPagamento { get; set; } = FormaPagamentoEnum.Pix;

    public string? Comprovante { get; set; }
    public string? Observacoes { get; set; }
}
