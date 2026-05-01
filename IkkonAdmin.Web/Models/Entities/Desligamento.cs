using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class Desligamento
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public DateOnly DataSolicitacao { get; set; }

    [Required, StringLength(400)]
    public string Motivo { get; set; } = string.Empty;

    public decimal PendenciaFinanceira { get; set; }
    public decimal MultaRescisoria { get; set; }

    public bool RequerimentoRecebido { get; set; }
    public DateOnly? DataConfirmacao { get; set; }

    public bool AcessosRemovidos { get; set; }
    public string? Observacoes { get; set; }
}
