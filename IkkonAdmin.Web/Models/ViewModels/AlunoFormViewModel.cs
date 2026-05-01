using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AlunoFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(150)]
    public string NomeCompleto { get; set; } = string.Empty;

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

    public DateOnly DataEntrada { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public int? TurmaId { get; set; }
    public StatusAlunoEnum Status { get; set; } = StatusAlunoEnum.EmAdmissao;

    public string? Observacoes { get; set; }
}
