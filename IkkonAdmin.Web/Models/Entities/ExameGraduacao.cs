using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class ExameGraduacao
{
    public int Id { get; set; }

    public DateOnly DataExame { get; set; }

    [StringLength(150)]
    public string? Local { get; set; }

    public NivelGraduacaoEnum NivelPretendido { get; set; } = NivelGraduacaoEnum.Basico;

    public string? Observacoes { get; set; }

    public ICollection<Graduacao> Graduacoes { get; } = new List<Graduacao>();
}
