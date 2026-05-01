using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AlunoIndexViewModel
{
    public string? Busca { get; set; }
    public StatusAlunoEnum? Status { get; set; }
    public int? TurmaId { get; set; }
    public int PaginaAtual { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public int TotalRegistros { get; set; }
    public int TotalPaginas => TamanhoPagina <= 0 ? 1 : (int)Math.Ceiling(TotalRegistros / (double)TamanhoPagina);

    public IReadOnlyCollection<TurmaFiltroViewModel> Turmas { get; set; } = [];
    public IReadOnlyCollection<AlunoListItemViewModel> Alunos { get; set; } = [];
}

public class TurmaFiltroViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class AlunoListItemViewModel
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? Celular { get; set; }
    public string? Turma { get; set; }
    public StatusAlunoEnum Status { get; set; }
    public DateOnly DataEntrada { get; set; }
}
