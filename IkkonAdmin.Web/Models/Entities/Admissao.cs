using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class Admissao
{
    public int Id { get; set; }

    public int? AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    [Required, StringLength(150)]
    public string NomeInteressado { get; set; } = string.Empty;

    public DateOnly DataAulaExperimental { get; set; }
    public DateOnly? DataMatricula { get; set; }

    public StatusAdmissaoEnum Status { get; set; } = StatusAdmissaoEnum.AulaExperimentalAgendada;

    public bool ContratoAssinado { get; set; }
    public bool PagamentoInicialConfirmado { get; set; }
    public bool IntegracaoConcluida { get; set; }

    public string? ChecklistObservacoes { get; set; }
}
