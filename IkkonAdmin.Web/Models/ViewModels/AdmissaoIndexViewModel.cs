using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AdmissaoIndexViewModel
{
    public string? Busca { get; set; }
    public StatusAdmissaoEnum? Status { get; set; }
    public IReadOnlyCollection<AdmissaoListItemViewModel> Admissoes { get; set; } = [];
}

public class AdmissaoListItemViewModel
{
    public int Id { get; set; }
    public string NomeInteressado { get; set; } = string.Empty;
    public DateOnly DataAulaExperimental { get; set; }
    public DateOnly? DataMatricula { get; set; }
    public StatusAdmissaoEnum Status { get; set; }
    public bool ContratoAssinado { get; set; }
    public bool PagamentoInicialConfirmado { get; set; }
    public bool IntegracaoConcluida { get; set; }
    public int? AlunoId { get; set; }
    public string? AlunoNome { get; set; }

    public int ChecklistConcluido =>
        (ContratoAssinado ? 1 : 0) +
        (PagamentoInicialConfirmado ? 1 : 0) +
        (IntegracaoConcluida ? 1 : 0);
}
