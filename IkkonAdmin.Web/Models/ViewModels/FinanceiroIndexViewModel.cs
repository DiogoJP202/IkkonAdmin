using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class FinanceiroIndexViewModel
{
    public int Pendentes { get; set; }
    public int Atrasadas { get; set; }
    public decimal ValorRecebidoMes { get; set; }
    public decimal ValorEmAberto { get; set; }

    public string? BuscaAluno { get; set; }
    public StatusMensalidadeEnum? StatusFiltro { get; set; }
    public int PaginaAtual { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public int TotalRegistros { get; set; }
    public int TotalPaginas => TamanhoPagina <= 0 ? 1 : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);
    public int MesCompetenciaGeracao { get; set; }
    public int AnoCompetenciaGeracao { get; set; }

    public IReadOnlyCollection<FinanceiroMensalidadeItemViewModel> Mensalidades { get; set; } = [];
}

public class FinanceiroMensalidadeItemViewModel
{
    public int MensalidadeId { get; set; }
    public int AlunoId { get; set; }
    public string Aluno { get; set; } = string.Empty;
    public DateOnly Competencia { get; set; }
    public DateOnly DataVencimento { get; set; }
    public DateOnly? DataPagamento { get; set; }
    public decimal ValorBase { get; set; }
    public decimal ValorFinal { get; set; }
    public StatusMensalidadeEnum Status { get; set; }
}

public class FinanceiroAtrasadosViewModel
{
    public decimal TotalEmAtraso { get; set; }
    public IReadOnlyCollection<FinanceiroAtrasadoItemViewModel> Itens { get; set; } = [];
}

public class FinanceiroAtrasadoItemViewModel
{
    public int MensalidadeId { get; set; }
    public int AlunoId { get; set; }
    public string Aluno { get; set; } = string.Empty;
    public DateOnly Competencia { get; set; }
    public DateOnly DataVencimento { get; set; }
    public int DiasAtraso { get; set; }
    public decimal ValorFinal { get; set; }
}

public class FinanceiroGeracaoResultadoViewModel
{
    public int Criadas { get; set; }
    public int JaExistentes { get; set; }
}
