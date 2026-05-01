using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class GraduacaoIndexViewModel
{
    public string? Busca { get; set; }
    public bool? ApenasAprovados { get; set; }
    public IReadOnlyCollection<GraduacaoListItemViewModel> Graduacoes { get; set; } = [];
    public IReadOnlyCollection<ExameGraduacaoListItemViewModel> Exames { get; set; } = [];
    public IReadOnlyCollection<GraduacaoAlunoAptoViewModel> AlunosAptos { get; set; } = [];
    public ExameGraduacaoCreateViewModel NovoExame { get; set; } = new();
}

public class GraduacaoListItemViewModel
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string? TurmaNome { get; set; }
    public DateOnly DataResultado { get; set; }
    public bool ResultadoAprovado { get; set; }
    public NivelGraduacaoEnum NivelAnterior { get; set; }
    public NivelGraduacaoEnum? NivelNovo { get; set; }
    public bool CertificadoEmitido { get; set; }
    public bool OmamoriAtualizado { get; set; }
    public int? ExameGraduacaoId { get; set; }
    public DateOnly? DataExame { get; set; }
}

public class ExameGraduacaoListItemViewModel
{
    public int Id { get; set; }
    public DateOnly DataExame { get; set; }
    public string? Local { get; set; }
    public NivelGraduacaoEnum NivelPretendido { get; set; }
    public int ResultadosRegistrados { get; set; }
}

public class GraduacaoAlunoAptoViewModel
{
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string? TurmaNome { get; set; }
    public NivelGraduacaoEnum NivelAtual { get; set; } = NivelGraduacaoEnum.Iniciante;
    public DateOnly? UltimoResultado { get; set; }
}

public class GraduacaoViewModel
{
    [Required]
    public int AlunoId { get; set; }

    public int? ExameGraduacaoId { get; set; }

    public DateOnly? DataExameNovo { get; set; }

    [StringLength(150)]
    public string? LocalExameNovo { get; set; }

    public NivelGraduacaoEnum NivelPretendidoExameNovo { get; set; } = NivelGraduacaoEnum.Basico;

    public DateOnly DataResultado { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public bool ResultadoAprovado { get; set; } = true;

    public NivelGraduacaoEnum? NivelNovo { get; set; }

    public bool CertificadoEmitido { get; set; }
    public bool OmamoriAtualizado { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }

    public IReadOnlyCollection<GraduacaoAlunoOpcaoViewModel> AlunosDisponiveis { get; set; } = [];
    public IReadOnlyCollection<GraduacaoExameOpcaoViewModel> ExamesDisponiveis { get; set; } = [];
}

public class GraduacaoAlunoOpcaoViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? TurmaNome { get; set; }
    public NivelGraduacaoEnum NivelAtual { get; set; }
}

public class GraduacaoExameOpcaoViewModel
{
    public int Id { get; set; }
    public DateOnly DataExame { get; set; }
    public string? Local { get; set; }
    public NivelGraduacaoEnum NivelPretendido { get; set; }
}

public class ExameGraduacaoCreateViewModel
{
    public DateOnly DataExame { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [StringLength(150)]
    public string? Local { get; set; }

    public NivelGraduacaoEnum NivelPretendido { get; set; } = NivelGraduacaoEnum.Basico;

    [StringLength(500)]
    public string? Observacoes { get; set; }
}

public class GraduacaoDetalhesViewModel
{
    public int Id { get; set; }
    public int AlunoId { get; set; }
    public string AlunoNome { get; set; } = string.Empty;
    public string? TurmaNome { get; set; }
    public DateOnly DataResultado { get; set; }
    public bool ResultadoAprovado { get; set; }
    public NivelGraduacaoEnum NivelAnterior { get; set; }
    public NivelGraduacaoEnum? NivelNovo { get; set; }
    public bool CertificadoEmitido { get; set; }
    public bool OmamoriAtualizado { get; set; }
    public string? Observacoes { get; set; }
    public int? ExameGraduacaoId { get; set; }
    public DateOnly? DataExame { get; set; }
    public string? LocalExame { get; set; }
    public NivelGraduacaoEnum? NivelPretendidoExame { get; set; }
    public IReadOnlyCollection<GraduacaoDetalhesHistoricoViewModel> HistoricoAluno { get; set; } = [];
}

public class GraduacaoDetalhesHistoricoViewModel
{
    public int Id { get; set; }
    public DateOnly DataResultado { get; set; }
    public bool ResultadoAprovado { get; set; }
    public NivelGraduacaoEnum NivelAnterior { get; set; }
    public NivelGraduacaoEnum? NivelNovo { get; set; }
}
