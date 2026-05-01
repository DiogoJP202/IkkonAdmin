using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class UserSettingsPageViewModel
{
    public AccountInfoViewModel AccountInfo { get; set; } = new();
    public SecurityStatusViewModel SecurityStatus { get; set; } = new();
    public PreferencesViewModel Preferences { get; set; } = new();
    public AccountTypeInfoViewModel AccountType { get; set; } = new();
}

public class AccountInfoViewModel
{
    [Required(ErrorMessage = "Informe seu nome completo.")]
    [StringLength(200, ErrorMessage = "Nome completo deve ter no máximo 200 caracteres.")]
    [Display(Name = "Nome completo")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [StringLength(150)]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    [Phone(ErrorMessage = "Informe um telefone válido.")]
    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    public string? FotoPerfilUrl { get; set; }
    public bool ContaAtiva { get; set; }
}

public class UpdateAccountInfoRequest
{
    [Required(ErrorMessage = "Informe seu nome completo.")]
    [StringLength(200, ErrorMessage = "Nome completo deve ter no máximo 200 caracteres.")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [StringLength(150)]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    [Phone(ErrorMessage = "Informe um telefone válido.")]
    public string? Telefone { get; set; }

    public IFormFile? FotoPerfil { get; set; }
}

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Informe sua senha atual.")]
    [DataType(DataType.Password)]
    public string SenhaAtual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A nova senha deve ter entre 8 e 100 caracteres.")]
    [DataType(DataType.Password)]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [Compare(nameof(NovaSenha), ErrorMessage = "A confirmação não confere com a nova senha.")]
    [DataType(DataType.Password)]
    public string ConfirmacaoNovaSenha { get; set; } = string.Empty;
}

public class SecurityStatusViewModel
{
    public bool ContaAtiva { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? UltimoLoginUtc { get; set; }
    public IReadOnlyCollection<HistoricoAcessoViewModel> HistoricoAcessos { get; set; } = Array.Empty<HistoricoAcessoViewModel>();
}

public class HistoricoAcessoViewModel
{
    public DateTime DataAcessoUtc { get; set; }
    public string? EnderecoIp { get; set; }
    public string Descricao { get; set; } = string.Empty;
}

public class PreferencesViewModel
{
    [Display(Name = "Tema")]
    public TemaPreferenciaEnum TemaPreferencia { get; set; } = TemaPreferenciaEnum.Claro;

    [Display(Name = "Idioma")]
    public IdiomaPreferenciaEnum IdiomaPreferencia { get; set; } = IdiomaPreferenciaEnum.PtBr;

    [Display(Name = "Notificações por e-mail")]
    public bool NotificarEmail { get; set; } = true;

    [Display(Name = "Notificações no sistema")]
    public bool NotificarSistema { get; set; } = true;
}

public class UpdatePreferencesRequest
{
    public TemaPreferenciaEnum TemaPreferencia { get; set; } = TemaPreferenciaEnum.Claro;
    public IdiomaPreferenciaEnum IdiomaPreferencia { get; set; } = IdiomaPreferenciaEnum.PtBr;
    public bool NotificarEmail { get; set; } = true;
    public bool NotificarSistema { get; set; } = true;
}

public class AccountTypeInfoViewModel
{
    public TipoAcessoEnum TipoAcesso { get; set; }
    public string NomeTipoConta { get; set; } = string.Empty;
    public IReadOnlyCollection<string> PermissoesBasicas { get; set; } = Array.Empty<string>();
}

public sealed record UserSettingsOperationResult(bool Success, string Message)
{
    public static UserSettingsOperationResult Ok(string message) => new(true, message);
    public static UserSettingsOperationResult Fail(string message) => new(false, message);
}
