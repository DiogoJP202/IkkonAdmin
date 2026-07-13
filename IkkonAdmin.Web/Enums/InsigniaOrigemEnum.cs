using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Enums;

public enum InsigniaOrigemEnum
{
    [Display(Name = "Manual")]
    Manual = 1,

    [Display(Name = "Automática")]
    Automatica = 2
}
