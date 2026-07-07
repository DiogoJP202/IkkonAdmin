using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Enums;

public enum ComunicadoAlvoTipoEnum
{
    [Display(Name = "Todos")]
    Todos = 1,

    [Display(Name = "Turma")]
    Turma = 2,

    [Display(Name = "Aluno")]
    Aluno = 3
}
