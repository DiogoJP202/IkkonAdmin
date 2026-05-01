namespace IkkonAdmin.Web.Models.ViewModels;

public class TurmaIndexViewModel
{
    public string? Busca { get; set; }
    public bool? Ativa { get; set; }
    public IReadOnlyCollection<TurmaListItemViewModel> Turmas { get; set; } = [];
}

public class TurmaListItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Modalidade { get; set; } = string.Empty;
    public string? Horario { get; set; }
    public bool Ativa { get; set; }
    public int QuantidadeAlunos { get; set; }
}
