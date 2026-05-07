using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Enums;

public enum GoogleAgendaTipoEventoEnum
{
    [Display(Name = "Aula")]
    Aula = 1,

    [Display(Name = "Ensaio")]
    Ensaio = 2,

    [Display(Name = "Apresentação")]
    Apresentacao = 3,

    [Display(Name = "Evento cultural")]
    EventoCultural = 4,

    [Display(Name = "Reunião")]
    Reuniao = 5,

    [Display(Name = "Outro")]
    Outro = 99
}
