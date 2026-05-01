using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AlunoDetalhesViewModel
{
    public int Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? RG { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public string? Endereco { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? ContatoEmergencia { get; set; }
    public DateOnly DataEntrada { get; set; }
    public string? Turma { get; set; }
    public StatusAlunoEnum Status { get; set; }
    public string? Observacoes { get; set; }

    public decimal TotalPago { get; set; }
    public decimal TotalEmAberto { get; set; }
    public int MensalidadesAtrasadas { get; set; }

    public IReadOnlyCollection<AlunoMensalidadeViewModel> Mensalidades { get; set; } = [];
    public IReadOnlyCollection<AlunoHistoricoItemViewModel> Historico { get; set; } = [];
}

public class AlunoMensalidadeViewModel
{
    public int Id { get; set; }
    public DateOnly Competencia { get; set; }
    public DateOnly DataVencimento { get; set; }
    public decimal ValorFinal { get; set; }
    public StatusMensalidadeEnum Status { get; set; }
    public DateOnly? DataPagamento { get; set; }
}

public class AlunoHistoricoItemViewModel
{
    public DateTime DataEvento { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
