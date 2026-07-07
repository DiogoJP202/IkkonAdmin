using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Enums;

public enum StatusAulaEnum
{
    [Display(Name = "Agendada")]
    Agendada = 1,

    [Display(Name = "Realizada")]
    Realizada = 2,

    [Display(Name = "Cancelada")]
    Cancelada = 3
}
