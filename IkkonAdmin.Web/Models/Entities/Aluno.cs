using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class Aluno
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string NomeCompleto { get; set; } = string.Empty;

    public DateOnly? DataNascimento { get; set; }

    [StringLength(20)]
    public string? RG { get; set; }

    [Required, StringLength(14)]
    public string CPF { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Endereco { get; set; }

    [StringLength(20)]
    public string? Celular { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(150)]
    public string? ContatoEmergencia { get; set; }

    public DateOnly DataEntrada { get; set; }

    public StatusAlunoEnum Status { get; set; } = StatusAlunoEnum.EmAdmissao;

    public string? Observacoes { get; set; }

    public int? TurmaId { get; set; }
    public Turma? Turma { get; set; }
    public ICollection<AlunoTurma> AlunoTurmas { get; } = new List<AlunoTurma>();

    public ICollection<Mensalidade> Mensalidades { get; } = new List<Mensalidade>();
    public ICollection<Pagamento> Pagamentos { get; } = new List<Pagamento>();
    public ICollection<Desconto> Descontos { get; } = new List<Desconto>();
    public ICollection<AcordoFinanceiro> AcordosFinanceiros { get; } = new List<AcordoFinanceiro>();
    public ICollection<Admissao> Admissoes { get; } = new List<Admissao>();
    public ICollection<Desligamento> Desligamentos { get; } = new List<Desligamento>();
    public ICollection<Graduacao> Graduacoes { get; } = new List<Graduacao>();
    public ICollection<HistoricoAluno> Historicos { get; } = new List<HistoricoAluno>();
}
