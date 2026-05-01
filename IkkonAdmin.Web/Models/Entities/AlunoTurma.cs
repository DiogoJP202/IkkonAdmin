namespace IkkonAdmin.Web.Models.Entities;

public class AlunoTurma
{
    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;

    public int TurmaId { get; set; }
    public Turma Turma { get; set; } = null!;

    public DateTime DataVinculo { get; set; } = DateTime.UtcNow;
}
