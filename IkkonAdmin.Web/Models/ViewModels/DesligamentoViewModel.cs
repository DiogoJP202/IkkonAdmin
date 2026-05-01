using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class DesligamentoIndexViewModel
{
    public string? Busca { get; set; }
    public bool? Confirmado { get; set; }
    public IReadOnlyCollection<DesligamentoListItemViewModel> Desligamentos { get; set; } = [];
}

public class DesligamentoListItemViewModel
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public StatusAlunoEnum StatusAluno { get; set; }
    public DateOnly DataSolicitacao { get; set; }
    public DateOnly? DataConfirmacao { get; set; }
    public decimal PendenciaFinanceira { get; set; }
    public decimal MultaRescisoria { get; set; }
    public bool RequerimentoRecebido { get; set; }
    public bool AcessosRemovidos { get; set; }
    public bool Confirmado => DataConfirmacao.HasValue;
}

public class DesligamentoCreateViewModel
{
    [Required]
    public int AlunoId { get; set; }

    public DateOnly DataSolicitacao { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required, StringLength(400)]
    public string Motivo { get; set; } = string.Empty;

    [Range(0, 999999)]
    public decimal PendenciaFinanceira { get; set; }

    [Range(0, 999999)]
    public decimal MultaRescisoria { get; set; }

    public bool RequerimentoRecebido { get; set; }
    public bool AcessosRemovidos { get; set; }
    public bool CalcularPendenciasAutomaticamente { get; set; } = true;
    public string? Observacoes { get; set; }

    public IReadOnlyCollection<DesligamentoAlunoOpcaoViewModel> AlunosDisponiveis { get; set; } = [];
}

public class DesligamentoDetalhesViewModel
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public StatusAlunoEnum StatusAluno { get; set; }

    public DateOnly DataSolicitacao { get; set; }
    public DateOnly? DataConfirmacao { get; set; }

    [Required, StringLength(400)]
    public string Motivo { get; set; } = string.Empty;

    [Range(0, 999999)]
    public decimal PendenciaFinanceira { get; set; }

    [Range(0, 999999)]
    public decimal MultaRescisoria { get; set; }

    public bool RequerimentoRecebido { get; set; }
    public bool AcessosRemovidos { get; set; }
    public string? Observacoes { get; set; }

    public bool Confirmado => DataConfirmacao.HasValue;
}

public class DesligamentoAlunoOpcaoViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public StatusAlunoEnum Status { get; set; }
    public string? Turma { get; set; }
}
