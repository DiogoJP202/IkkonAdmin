namespace IkkonAdmin.Web.Models.Entities;

public class ComunicadoAlvo
{
    public int Id { get; set; }

    public int ComunicadoId { get; set; }
    public Comunicado? Comunicado { get; set; }

    public int? AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public bool Todos { get; set; }
}
