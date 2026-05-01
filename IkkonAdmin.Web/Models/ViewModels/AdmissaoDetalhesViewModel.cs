using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AdmissaoDetalhesViewModel
{
    public int Id { get; set; }
    public string NomeInteressado { get; set; } = string.Empty;
    public DateOnly DataAulaExperimental { get; set; }
    public DateOnly? DataMatricula { get; set; }
    public StatusAdmissaoEnum Status { get; set; }
    public bool ContratoAssinado { get; set; }
    public bool PagamentoInicialConfirmado { get; set; }
    public bool IntegracaoConcluida { get; set; }
    public string? ChecklistObservacoes { get; set; }

    public int? AlunoId { get; set; }
    public string? AlunoNome { get; set; }

    public IReadOnlyCollection<AdmissaoTurmaOpcaoViewModel> Turmas { get; set; } = [];
    public AdmissaoMatriculaViewModel Matricula { get; set; } = new();
}

public class AdmissaoMatriculaViewModel
{
    [Required, StringLength(14)]
    public string CPF { get; set; } = string.Empty;

    [StringLength(20)]
    public string? RG { get; set; }

    public DateOnly? DataNascimento { get; set; }

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(20)]
    public string? Celular { get; set; }

    [StringLength(150), EmailAddress]
    public string? Email { get; set; }

    [StringLength(150)]
    public string? ContatoEmergencia { get; set; }

    public int? TurmaId { get; set; }
    public string? ObservacoesAluno { get; set; }
}

public class AdmissaoTurmaOpcaoViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
