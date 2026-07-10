using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AreaAlunoAdminDashboardViewModel
{
    public int AulasProximas { get; set; }
    public int FrequenciasRegistradasMes { get; set; }
    public int DocumentosPendentes { get; set; }
    public int ComunicadosAtivos { get; set; }
    public int EventosProximos { get; set; }
    public int ConquistasConcedidasMes { get; set; }
    public IReadOnlyCollection<AreaAlunoAulaAdminItemViewModel> ProximasAulas { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoDocumentoAdminItemViewModel> DocumentosRecentes { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoComunicadoAdminItemViewModel> ComunicadosRecentes { get; set; } = [];
}

public class AreaAlunoAulasAdminViewModel
{
    public TurmaHorarioFormViewModel NovoHorario { get; set; } = new();
    public TurmaInstrutorFormViewModel NovoInstrutor { get; set; } = new();
    public AulaFormViewModel NovaAula { get; set; } = new();
    public IReadOnlyCollection<AreaAlunoOpcaoViewModel> Turmas { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoOpcaoViewModel> Instrutores { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoHorarioAdminItemViewModel> Horarios { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoInstrutorAdminItemViewModel> TurmaInstrutores { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoAulaAdminItemViewModel> Aulas { get; set; } = [];
}

public class AreaAlunoFrequenciaAdminViewModel
{
    public IReadOnlyCollection<AreaAlunoAulaAdminItemViewModel> Aulas { get; set; } = [];
}

public class AreaAlunoRegistroFrequenciaViewModel
{
    public int AulaId { get; set; }
    public string Turma { get; set; } = string.Empty;
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string? Instrutor { get; set; }
    public IReadOnlyCollection<FrequenciaRegistroItemViewModel> Alunos { get; set; } = [];
}

public class FrequenciaRegistroPostViewModel
{
    public int AulaId { get; set; }
    public List<FrequenciaRegistroItemViewModel> Alunos { get; set; } = [];
}

public class FrequenciaRegistroItemViewModel
{
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public StatusFrequenciaEnum Status { get; set; } = StatusFrequenciaEnum.Presente;
    public bool Justificada { get; set; }

    [StringLength(500)]
    public string? Justificativa { get; set; }
}

public class AreaAlunoDocumentosAdminViewModel
{
    public DocumentoTipoFormViewModel NovoTipo { get; set; } = new();
    public DocumentoSolicitacaoFormViewModel NovaSolicitacao { get; set; } = new();
    public IReadOnlyCollection<AreaAlunoOpcaoViewModel> Alunos { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoDocumentoTipoItemViewModel> Tipos { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoDocumentoAdminItemViewModel> Solicitacoes { get; set; } = [];
}

public class AreaAlunoComunicadosAdminViewModel
{
    public ComunicadoFormViewModel NovoComunicado { get; set; } = new();
    public IReadOnlyCollection<AreaAlunoOpcaoViewModel> Alunos { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoOpcaoViewModel> Turmas { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoComunicadoAdminItemViewModel> Comunicados { get; set; } = [];
}

public class AreaAlunoEventosAdminViewModel
{
    public EventoAlunoFormViewModel NovoEvento { get; set; } = new();
    public IReadOnlyCollection<AreaAlunoOpcaoViewModel> Alunos { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoOpcaoViewModel> Turmas { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoEventoAdminItemViewModel> Eventos { get; set; } = [];
}

public class AreaAlunoConquistasAdminViewModel
{
    public InsigniaFormViewModel NovaInsignia { get; set; } = new();
    public AlunoInsigniaFormViewModel NovaAtribuicao { get; set; } = new();
    public IReadOnlyCollection<AreaAlunoOpcaoViewModel> Alunos { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoInsigniaItemViewModel> Insignias { get; set; } = [];
    public IReadOnlyCollection<AreaAlunoConquistaAdminItemViewModel> Conquistas { get; set; } = [];
}

public class AreaAlunoOpcaoViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class TurmaHorarioFormViewModel
{
    [Required(ErrorMessage = "Selecione a turma.")]
    public int TurmaId { get; set; }

    public DayOfWeek DiaSemana { get; set; } = DayOfWeek.Monday;

    [Required(ErrorMessage = "Informe o horario inicial.")]
    public TimeOnly HoraInicio { get; set; } = new(19, 0);

    [Required(ErrorMessage = "Informe o horario final.")]
    public TimeOnly HoraFim { get; set; } = new(20, 30);

    [StringLength(150)]
    public string? Local { get; set; }
}

public class TurmaInstrutorFormViewModel
{
    [Required(ErrorMessage = "Selecione a turma.")]
    public int TurmaId { get; set; }

    [Required(ErrorMessage = "Selecione o instrutor.")]
    public int UsuarioSistemaId { get; set; }

    public bool Principal { get; set; } = true;
    public DateOnly DataInicio { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public DateOnly? DataFim { get; set; }
}

public class AulaFormViewModel
{
    [Required(ErrorMessage = "Selecione a turma.")]
    public int TurmaId { get; set; }

    public int? TurmaHorarioId { get; set; }
    public int? InstrutorUsuarioId { get; set; }

    [Required(ErrorMessage = "Informe o inicio da aula.")]
    public DateTime Inicio { get; set; } = DateTime.Today.AddHours(19);

    [Required(ErrorMessage = "Informe o fim da aula.")]
    public DateTime Fim { get; set; } = DateTime.Today.AddHours(20).AddMinutes(30);

    [StringLength(150)]
    public string? Local { get; set; }

    public StatusAulaEnum Status { get; set; } = StatusAulaEnum.Agendada;
    public string? Observacoes { get; set; }
}

public class DocumentoTipoFormViewModel
{
    [Required(ErrorMessage = "Informe o nome do tipo.")]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    public bool Obrigatorio { get; set; }
    public bool Ativo { get; set; } = true;
}

public class DocumentoSolicitacaoFormViewModel
{
    [Required(ErrorMessage = "Selecione o tipo.")]
    public int DocumentoTipoId { get; set; }

    [Required(ErrorMessage = "Selecione o aluno.")]
    public int AlunoId { get; set; }

    public DateOnly? DataLimite { get; set; }

    [StringLength(1000)]
    public string? ObservacaoAdministrativa { get; set; }
}

public class DocumentoSolicitacaoEdicaoViewModel
{
    [Required(ErrorMessage = "Selecione o tipo.")]
    public int DocumentoTipoId { get; set; }

    [Required(ErrorMessage = "Selecione o aluno.")]
    public int AlunoId { get; set; }

    public DocumentoStatusEnum Status { get; set; } = DocumentoStatusEnum.Solicitado;
    public DateOnly? DataLimite { get; set; }

    [StringLength(1000)]
    public string? ObservacaoAdministrativa { get; set; }
}

public class DocumentoAvaliacaoFormViewModel
{
    public int SolicitacaoId { get; set; }
    public DocumentoStatusEnum Status { get; set; }

    [StringLength(1000)]
    public string? ObservacaoAdministrativa { get; set; }
}

public class ComunicadoFormViewModel
{
    [Required(ErrorMessage = "Informe o titulo.")]
    [StringLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o conteudo.")]
    [StringLength(4000)]
    public string Conteudo { get; set; } = string.Empty;

    public bool Importante { get; set; }
    public bool Fixado { get; set; }
    public DateTime PublicadoEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiraEmUtc { get; set; }
    public ComunicadoAlvoTipoEnum AlvoTipo { get; set; } = ComunicadoAlvoTipoEnum.Todos;
    public int? AlunoId { get; set; }
    public int? TurmaId { get; set; }
}

public class EventoAlunoFormViewModel
{
    [Required(ErrorMessage = "Informe o titulo.")]
    [StringLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Descricao { get; set; }

    public DateTime Inicio { get; set; } = DateTime.Today.AddDays(7).AddHours(19);
    public DateTime Fim { get; set; } = DateTime.Today.AddDays(7).AddHours(21);

    [StringLength(180)]
    public string? Local { get; set; }

    public EventoAlunoTipoEnum Tipo { get; set; } = EventoAlunoTipoEnum.Atividade;
    public bool Importante { get; set; }

    [StringLength(200)]
    public string? GoogleEventoId { get; set; }

    public ComunicadoAlvoTipoEnum AlvoTipo { get; set; } = ComunicadoAlvoTipoEnum.Todos;
    public int? AlunoId { get; set; }
    public int? TurmaId { get; set; }
}

public class InsigniaFormViewModel
{
    [Required(ErrorMessage = "Informe o nome da insignia.")]
    [StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [StringLength(80)]
    public string? Icone { get; set; }

    [StringLength(80)]
    public string? Categoria { get; set; }

    [StringLength(120)]
    public string? RegraAutomatica { get; set; }

    public bool Ativa { get; set; } = true;
}

public class AlunoInsigniaFormViewModel
{
    [Required(ErrorMessage = "Selecione o aluno.")]
    public int AlunoId { get; set; }

    [Required(ErrorMessage = "Selecione a insignia.")]
    public int InsigniaId { get; set; }

    [StringLength(500)]
    public string? Observacao { get; set; }
}

public class AreaAlunoHorarioAdminItemViewModel
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public string Turma { get; set; } = string.Empty;
    public DayOfWeek DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFim { get; set; }
    public string? Local { get; set; }
    public bool Ativo { get; set; }
}

public class AreaAlunoInstrutorAdminItemViewModel
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public string Turma { get; set; } = string.Empty;
    public int UsuarioSistemaId { get; set; }
    public string Instrutor { get; set; } = string.Empty;
    public bool Principal { get; set; }
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
}

public class AreaAlunoAulaAdminItemViewModel
{
    public int Id { get; set; }
    public int TurmaId { get; set; }
    public string Turma { get; set; } = string.Empty;
    public int? TurmaHorarioId { get; set; }
    public int? InstrutorUsuarioId { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string? Local { get; set; }
    public string? Instrutor { get; set; }
    public StatusAulaEnum Status { get; set; }
    public string? Observacoes { get; set; }
    public int TotalAlunos { get; set; }
    public int FrequenciasRegistradas { get; set; }
}

public class AreaAlunoDocumentoTipoItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Obrigatorio { get; set; }
    public bool Ativo { get; set; }
}

public class AreaAlunoDocumentoAdminItemViewModel
{
    public int SolicitacaoId { get; set; }
    public int AlunoId { get; set; }
    public string Aluno { get; set; } = string.Empty;
    public int DocumentoTipoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DocumentoStatusEnum Status { get; set; }
    public DateTime DataSolicitacaoUtc { get; set; }
    public DateOnly? DataLimite { get; set; }
    public string? ObservacaoAdministrativa { get; set; }
    public int Envios { get; set; }
    public int? UltimoEnvioId { get; set; }
    public string? NomeArquivoOriginal { get; set; }
}

public class AreaAlunoComunicadoAdminItemViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Conteudo { get; set; } = string.Empty;
    public bool Importante { get; set; }
    public bool Fixado { get; set; }
    public bool Ativo { get; set; }
    public DateTime PublicadoEmUtc { get; set; }
    public DateTime? ExpiraEmUtc { get; set; }
    public ComunicadoAlvoTipoEnum AlvoTipo { get; set; } = ComunicadoAlvoTipoEnum.Todos;
    public int? AlunoId { get; set; }
    public int? TurmaId { get; set; }
    public int Leituras { get; set; }
}

public class AreaAlunoEventoAdminItemViewModel
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public string? Local { get; set; }
    public EventoAlunoTipoEnum Tipo { get; set; }
    public bool Importante { get; set; }
    public bool Ativo { get; set; }
    public string? GoogleEventoId { get; set; }
    public ComunicadoAlvoTipoEnum AlvoTipo { get; set; } = ComunicadoAlvoTipoEnum.Todos;
    public int? AlunoId { get; set; }
    public int? TurmaId { get; set; }
}

public class AreaAlunoInsigniaItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Icone { get; set; }
    public string? Categoria { get; set; }
    public string? RegraAutomatica { get; set; }
    public bool Ativa { get; set; }
}

public class AreaAlunoConquistaAdminItemViewModel
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string Aluno { get; set; } = string.Empty;
    public int InsigniaId { get; set; }
    public string Insignia { get; set; } = string.Empty;
    public DateTime ConcedidaEmUtc { get; set; }
    public InsigniaOrigemEnum Origem { get; set; }
    public string? Observacao { get; set; }
}
