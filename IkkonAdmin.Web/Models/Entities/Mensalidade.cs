using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class Mensalidade
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public DateOnly Competencia { get; set; }
    public DateOnly DataVencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }

    public decimal ValorBase { get; set; }
    public decimal ValorFinal { get; set; }

    public StatusMensalidadeEnum Status { get; set; } = StatusMensalidadeEnum.Pendente;

    public string? Observacoes { get; set; }

    public ICollection<Pagamento> Pagamentos { get; } = new List<Pagamento>();
}
