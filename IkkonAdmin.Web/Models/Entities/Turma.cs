using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class Turma
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Modalidade { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Horario { get; set; }

    public bool Ativa { get; set; } = true;

    public string? Observacoes { get; set; }

    public ICollection<Aluno> Alunos { get; } = new List<Aluno>();
    public ICollection<AlunoTurma> AlunoTurmas { get; } = new List<AlunoTurma>();
}
