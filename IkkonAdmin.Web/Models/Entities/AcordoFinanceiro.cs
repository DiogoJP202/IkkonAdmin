using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class AcordoFinanceiro
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    [Required, StringLength(150)]
    public string Descricao { get; set; } = string.Empty;

    public decimal ValorMensalAcordado { get; set; }

    public DateOnly InicioVigencia { get; set; }
    public DateOnly? FimVigencia { get; set; }

    public bool Ativo { get; set; } = true;
    public string? Observacoes { get; set; }
}
