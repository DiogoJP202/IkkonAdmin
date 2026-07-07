using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class Aula
{
    public int Id { get; set; }

    public int TurmaId { get; set; }
    public Turma? Turma { get; set; }

    public int? TurmaHorarioId { get; set; }
    public TurmaHorario? TurmaHorario { get; set; }

    public int? InstrutorUsuarioId { get; set; }
    public UsuarioSistema? InstrutorUsuario { get; set; }

    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }

    [StringLength(150)]
    public string? Local { get; set; }

    public StatusAulaEnum Status { get; set; } = StatusAulaEnum.Agendada;
    public string? Observacoes { get; set; }

    public ICollection<FrequenciaAluno> Frequencias { get; } = new List<FrequenciaAluno>();
}
