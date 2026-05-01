using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class Desconto
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    [Required, StringLength(80)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Tipo { get; set; }

    public decimal? Percentual { get; set; }
    public decimal? ValorFixo { get; set; }

    public DateOnly VigenciaInicio { get; set; }
    public DateOnly? VigenciaFim { get; set; }

    public bool Ativo { get; set; } = true;
    public string? Observacoes { get; set; }
}
