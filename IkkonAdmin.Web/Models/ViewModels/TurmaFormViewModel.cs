using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class TurmaFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Modalidade { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Horario { get; set; }

    public bool Ativa { get; set; } = true;
    public string? Observacoes { get; set; }

    public List<int> AlunosIds { get; set; } = [];
    public IReadOnlyCollection<TurmaAlunoOpcaoViewModel> AlunosDisponiveis { get; set; } = [];
}

public class TurmaAlunoOpcaoViewModel
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public StatusAlunoEnum Status { get; set; }
    public string? TurmaAtual { get; set; }
}
