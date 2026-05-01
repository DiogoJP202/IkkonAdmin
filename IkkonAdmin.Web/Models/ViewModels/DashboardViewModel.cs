using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class DashboardViewModel
{
    public int QuantidadeAlunosAtivos { get; set; }
    public int MensalidadesPendentes { get; set; }
    public int MensalidadesAtrasadas { get; set; }
    public decimal ReceitaRecebidaNoMes { get; set; }
    public decimal TotalEmAtraso { get; set; }
    public int QuantidadeAlunosInadimplentes { get; set; }

    public int AnoReferencia { get; set; }
    public int MesReferencia { get; set; }
    public string MesAnoReferenciaDescricao { get; set; } = string.Empty;

    public int? TurmaIdFiltro { get; set; }
    public IReadOnlyCollection<DashboardTurmaFiltroViewModel> TurmasDisponiveis { get; set; } = [];

    public IReadOnlyCollection<ProximoVencimentoViewModel> ProximosVencimentos { get; set; } = [];
    public IReadOnlyCollection<InadimplenteResumoViewModel> Inadimplentes { get; set; } = [];
    public IReadOnlyCollection<AtividadeRecenteViewModel> AtividadesRecentes { get; set; } = [];
}

public class DashboardTurmaFiltroViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class ProximoVencimentoViewModel
{
    public int MensalidadeId { get; set; }
    public int AlunoId { get; set; }
    public string Aluno { get; set; } = string.Empty;
    public string? Turma { get; set; }
    public DateOnly Vencimento { get; set; }
    public decimal Valor { get; set; }
    public StatusMensalidadeEnum Status { get; set; }
    public int DiasParaVencimento { get; set; }
}

public class InadimplenteResumoViewModel
{
    public int AlunoId { get; set; }
    public string Aluno { get; set; } = string.Empty;
    public string? Turma { get; set; }
    public int QuantidadeMensalidades { get; set; }
    public decimal TotalEmAberto { get; set; }
    public int MaiorDiasAtraso { get; set; }
}

public class AtividadeRecenteViewModel
{
    public int? AlunoId { get; set; }
    public DateTime Data { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
