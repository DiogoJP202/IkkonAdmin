using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class EventoAlunoPortal
{
    public int Id { get; set; }

    [Required, StringLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Descricao { get; set; }

    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }

    [StringLength(180)]
    public string? Local { get; set; }

    public EventoAlunoTipoEnum Tipo { get; set; } = EventoAlunoTipoEnum.Outro;
    public bool Importante { get; set; }
    public bool Ativo { get; set; } = true;

    [StringLength(200)]
    public string? GoogleEventoId { get; set; }

    public ICollection<EventoAlunoPortalAlvo> Alvos { get; } = new List<EventoAlunoPortalAlvo>();
}
