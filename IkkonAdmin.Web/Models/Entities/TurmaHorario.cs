using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class TurmaHorario
{
    public int Id { get; set; }

    public int TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }

    [StringLength(150)]
    public string? Local { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<Aula> Aulas { get; } = new List<Aula>();
}
