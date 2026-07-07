using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Enums;

public enum StatusFrequenciaEnum
{
    [Display(Name = "Presente")]
    Presente = 1,

    [Display(Name = "Falta")]
    Falta = 2,

    [Display(Name = "Falta justificada")]
    FaltaJustificada = 3,

    [Display(Name = "Cancelada")]
    Cancelada = 4
}
