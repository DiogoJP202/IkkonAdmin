using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class FrequenciaAluno
{
    public int Id { get; set; }

    public int AulaId { get; set; }
    public Aula? Aula { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public StatusFrequenciaEnum Status { get; set; } = StatusFrequenciaEnum.Presente;
    public bool Justificada { get; set; }

    [StringLength(500)]
    public string? Justificativa { get; set; }

    public int? RegistradoPorUsuarioId { get; set; }
    public UsuarioSistema? RegistradoPorUsuario { get; set; }

    public DateTime RegistradoEmUtc { get; set; } = DateTime.UtcNow;
}
