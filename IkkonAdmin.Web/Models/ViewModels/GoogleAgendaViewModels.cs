using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class GoogleAgendaFiltroViewModel : IValidatableObject
{
    [Display(Name = "Início")]
    [DataType(DataType.Date)]
    public DateOnly Inicio { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(-7));

    [Display(Name = "Fim")]
    [DataType(DataType.Date)]
    public DateOnly Fim { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(45));

    [Display(Name = "Tipo")]
    public GoogleAgendaTipoEventoEnum? Tipo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Fim < Inicio)
        {
            yield return new ValidationResult("A data final deve ser maior ou igual à data inicial.", new[] { nameof(Fim) });
        }
    }
}

public class GoogleAgendaIndexViewModel
{
    public GoogleAgendaFiltroViewModel Filtro { get; set; } = new();
    public List<GoogleAgendaEventoViewModel> Eventos { get; set; } = new();
    public bool ConfiguracaoPendente { get; set; }
    public bool OAuthConectado { get; set; }
    public string? MensagemConfiguracao { get; set; }
    public string? CalendarId { get; set; }
}

public class GoogleAgendaEventoViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Local { get; set; }
    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public bool DiaInteiro { get; set; }
    public GoogleAgendaTipoEventoEnum Tipo { get; set; } = GoogleAgendaTipoEventoEnum.Outro;
    public string? Status { get; set; }
    public string? HtmlLink { get; set; }
}

public class GoogleAgendaEventoFormViewModel : IValidatableObject
{
    public string? Id { get; set; }

    [Display(Name = "Título")]
    [Required(ErrorMessage = "Informe o título do evento.")]
    [StringLength(180, ErrorMessage = "Título deve ter no máximo 180 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Tipo")]
    [Required(ErrorMessage = "Selecione o tipo do evento.")]
    public GoogleAgendaTipoEventoEnum Tipo { get; set; } = GoogleAgendaTipoEventoEnum.Aula;

    [Display(Name = "Início")]
    [Required(ErrorMessage = "Informe a data e hora de início.")]
    public DateTime Inicio { get; set; } = DateTime.Today.AddHours(19);

    [Display(Name = "Fim")]
    [Required(ErrorMessage = "Informe a data e hora de fim.")]
    public DateTime Fim { get; set; } = DateTime.Today.AddHours(21);

    [Display(Name = "Local")]
    [StringLength(180, ErrorMessage = "Local deve ter no máximo 180 caracteres.")]
    public string? Local { get; set; }

    [Display(Name = "Descrição")]
    [StringLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres.")]
    public string? Descricao { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Fim <= Inicio)
        {
            yield return new ValidationResult("A data final deve ser posterior à data inicial.", new[] { nameof(Fim) });
        }
    }
}
