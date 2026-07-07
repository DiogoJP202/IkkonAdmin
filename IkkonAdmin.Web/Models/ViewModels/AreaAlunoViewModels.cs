using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AreaAlunoDashboardViewModel
{
    public int AlunoId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Celular { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public StatusAlunoEnum Status { get; set; }
    public string? TurmaPrincipal { get; set; }
    public DateOnly DataEntrada { get; set; }
    public decimal TotalEmAberto { get; set; }
    public int MensalidadesAtrasadas { get; set; }
    public int DocumentosPendentes { get; set; }
    public int ComunicadosNaoLidos { get; set; }
    public int FaltasNaoJustificadas { get; set; }
    public decimal PercentualPresenca { get; set; }
    public IReadOnlyCollection<AreaAlunoTurmaItemViewModel> Turmas { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoMensalidadeItemViewModel> MensalidadesRecentes { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoAulaItemViewModel> ProximasAulas { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoEventoItemViewModel> ProximosEventos { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoDocumentoItemViewModel> DocumentosRecentes { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoComunicadoItemViewModel> ComunicadosRecentes { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoFrequenciaItemViewModel> FaltasRecentes { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoConquistaItemViewModel> ConquistasRecentes { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoAlertaViewModel> Alertas { get; set; } = [];
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
    public IReadOnlyCollection<AreaAlunoAulaItemViewModel> ProximasAulas { get; set; } = [];
}

public class AreaAlunoTurmaItemViewModel
{
    public string Nome { get; set; } = string.Empty;
    public string Modalidade { get; set; } = string.Empty;
    public string? Horario { get; set; }
    public string? Local { get; set; }
    public string? Instrutor { get; set; }
    public IReadOnlyCollection<AreaAlunoHorarioItemViewModel> Horarios { get; set; } = [];
    public DateTime DataVinculo { get; set; }
}

public class AreaAlunoMensalidadeItemViewModel
{
    public int Id { get; set; }
    public DateOnly Competencia { get; set; }
    public DateOnly DataVencimento { get; set; }
    public decimal ValorFinal { get; set; }
    public StatusMensalidadeEnum Status { get; set; }
    public DateOnly? DataPagamento { get; set; }
    public FormaPagamentoEnum? FormaPagamento { get; set; }
    public string? Comprovante { get; set; }
}

public class AreaAlunoHorarioItemViewModel
{
    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }
    public string? Local { get; set; }
}

public class AreaAlunoAulasViewModel
{
    public IReadOnlyCollection<AreaAlunoTurmaItemViewModel> Turmas { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoAulaItemViewModel> ProximasAulas { get; set; } = [];
}

public class AreaAlunoAulaItemViewModel
{
    public int Id { get; set; }
    public string Turma { get; set; } = string.Empty;
    public string Modalidade { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string? Local { get; set; }
    public string? Instrutor { get; set; }
    public StatusAulaEnum Status { get; set; }
}

public class AreaAlunoFrequenciaViewModel
{
    public DateOnly Inicio { get; set; }
    public DateOnly Fim { get; set; }
    public int TotalRegistros { get; set; }
    public int Presencas { get; set; }
    public int FaltasJustificadas { get; set; }
    public int FaltasNaoJustificadas { get; set; }
    public decimal PercentualPresenca { get; set; }
    public IReadOnlyCollection<AreaAlunoFrequenciaItemViewModel> Registros { get; set; } = [];
}

public class AreaAlunoFrequenciaItemViewModel
{
    public int AulaId { get; set; }
    public DateTime Inicio { get; set; }
    public string Turma { get; set; } = string.Empty;
    public string? Instrutor { get; set; }
    public StatusFrequenciaEnum Status { get; set; }
    public bool Justificada { get; set; }
    public string? Justificativa { get; set; }
}

public class AreaAlunoEventosViewModel
{
    public IReadOnlyCollection<AreaAlunoEventoItemViewModel> Eventos { get; set; } = [];
}

public class AreaAlunoEventoItemViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string? Local { get; set; }
    public EventoAlunoTipoEnum Tipo { get; set; }
    public bool Importante { get; set; }
}

public class AreaAlunoDocumentosViewModel
{
    public IReadOnlyCollection<AreaAlunoDocumentoItemViewModel> Documentos { get; set; } = [];
}

public class AreaAlunoDocumentoItemViewModel
{
    public int SolicitacaoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DocumentoStatusEnum Status { get; set; }
    public DateTime DataSolicitacaoUtc { get; set; }
    public DateOnly? DataLimite { get; set; }
    public string? ObservacaoAdministrativa { get; set; }
    public int? UltimoEnvioId { get; set; }
    public string? NomeArquivoOriginal { get; set; }
    public DateTime? EnviadoEmUtc { get; set; }
}

public class AreaAlunoComunicadosViewModel
{
    public IReadOnlyCollection<AreaAlunoComunicadoItemViewModel> Comunicados { get; set; } = [];
}

public class AreaAlunoComunicadoItemViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public bool Importante { get; set; }
    public bool Fixado { get; set; }
    public DateTime PublicadoEmUtc { get; set; }
    public DateTime? ExpiraEmUtc { get; set; }
    public bool Lido { get; set; }
}

public class AreaAlunoConquistasViewModel
{
    public IReadOnlyCollection<AreaAlunoConquistaItemViewModel> Conquistas { get; set; } = [];
}

public class AreaAlunoConquistaItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Icone { get; set; }
    public string? Categoria { get; set; }
    public DateTime ConcedidaEmUtc { get; set; }
    public InsigniaOrigemEnum Origem { get; set; }
    public string? Observacao { get; set; }
}

public class AreaAlunoAlertaViewModel
{
    public string Tipo { get; set; } = "info";
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string? Url { get; set; }
}
