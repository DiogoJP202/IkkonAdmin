using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Enums;

public enum DocumentoStatusEnum
{
    [Display(Name = "Solicitado")]
    Solicitado = 1,

    [Display(Name = "Enviado")]
    Enviado = 2,

    [Display(Name = "Aprovado")]
    Aprovado = 3,

    [Display(Name = "Recusado")]
    Recusado = 4,

    [Display(Name = "Pendente")]
    Pendente = 5
}
