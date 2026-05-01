using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AdmissaoViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(150)]
    public string NomeInteressado { get; set; } = string.Empty;

    public DateOnly DataAulaExperimental { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public StatusAdmissaoEnum Status { get; set; } = StatusAdmissaoEnum.AulaExperimentalAgendada;

    public bool ContratoAssinado { get; set; }
    public bool PagamentoInicialConfirmado { get; set; }
    public bool IntegracaoConcluida { get; set; }

    public string? ChecklistObservacoes { get; set; }
}
