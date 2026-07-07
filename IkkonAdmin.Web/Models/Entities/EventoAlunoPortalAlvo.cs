namespace IkkonAdmin.Web.Models.Entities;

public class EventoAlunoPortalAlvo
{
    public int Id { get; set; }

    public int EventoAlunoPortalId { get; set; }
    public EventoAlunoPortal? EventoAlunoPortal { get; set; }

    public int? AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public bool Todos { get; set; }
}
