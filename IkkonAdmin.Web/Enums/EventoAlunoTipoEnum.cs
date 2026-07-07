using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Enums;

public enum EventoAlunoTipoEnum
{
    [Display(Name = "Aula especial")]
    AulaEspecial = 1,

    [Display(Name = "Apresentacao")]
    Apresentacao = 2,

    [Display(Name = "Exame")]
    Exame = 3,

    [Display(Name = "Reuniao")]
    Reuniao = 4,

    [Display(Name = "Feriado")]
    Feriado = 5,

    [Display(Name = "Reposicao")]
    Reposicao = 6,

    [Display(Name = "Atividade")]
    Atividade = 7,

    [Display(Name = "Outro")]
    Outro = 99
}
