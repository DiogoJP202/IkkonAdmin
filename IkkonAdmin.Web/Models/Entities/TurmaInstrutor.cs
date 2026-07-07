namespace IkkonAdmin.Web.Models.Entities;

public class TurmaInstrutor
{
    public int Id { get; set; }

    public int TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public int UsuarioSistemaId { get; set; }
    public UsuarioSistema? UsuarioSistema { get; set; }

    public bool Principal { get; set; }
    public DateOnly DataInicio { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? DataFim { get; set; }
}
