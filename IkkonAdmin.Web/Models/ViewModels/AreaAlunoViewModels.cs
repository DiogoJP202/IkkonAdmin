using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AreaAlunoDashboardViewModel
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Celular { get; set; }
    public StatusAlunoEnum Status { get; set; }
    public string? TurmaPrincipal { get; set; }
    public DateOnly DataEntrada { get; set; }
    public decimal TotalEmAberto { get; set; }
    public int MensalidadesAtrasadas { get; set; }
    public IReadOnlyCollection<AreaAlunoTurmaItemViewModel> Turmas { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoMensalidadeItemViewModel> MensalidadesRecentes { get; set; } = [];
}

public class AreaAlunoPerfilViewModel
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? RG { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public string? Email { get; set; }
    public string? Celular { get; set; }
    public string? Endereco { get; set; }
    public string? ContatoEmergencia { get; set; }
    public DateOnly DataEntrada { get; set; }
    public StatusAlunoEnum Status { get; set; }
}

public class AreaAlunoFinanceiroViewModel
{
    public decimal TotalEmAberto { get; set; }
    public decimal TotalPago { get; set; }
    public int MensalidadesAtrasadas { get; set; }
    public IReadOnlyCollection<AreaAlunoMensalidadeItemViewModel> Mensalidades { get; set; } = [];
}

public class AreaAlunoTurmasViewModel
{
    public string? TurmaPrincipal { get; set; }
    public IReadOnlyCollection<AreaAlunoTurmaItemViewModel> Turmas { get; set; } = [];
}

public class AreaAlunoTurmaItemViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string Modalidade { get; set; } = string.Empty;
    public string? Horario { get; set; }
    public DateTime DataVinculo { get; set; }
}

public class AreaAlunoMensalidadeItemViewModel
{
    public DateOnly Competencia { get; set; }
    public DateOnly DataVencimento { get; set; }
    public decimal ValorFinal { get; set; }
    public StatusMensalidadeEnum Status { get; set; }
    public DateOnly? DataPagamento { get; set; }
}
