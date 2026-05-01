using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class LoginViewModel
{
    [Display(Name = "E-mail ou usu\u00E1rio")]
    [Required(ErrorMessage = "Informe seu e-mail ou usu\u00E1rio.")]
    [StringLength(150)]
    public string LoginOuEmail { get; set; } = string.Empty;

    [Display(Name = "Senha")]
    [Required(ErrorMessage = "Informe sua senha.")]
    [DataType(DataType.Password)]
    [StringLength(100)]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Tipo de acesso")]
    [Required(ErrorMessage = "Selecione o tipo de acesso.")]
    [EnumDataType(typeof(TipoAcessoEnum), ErrorMessage = "Selecione um tipo de acesso v\u00E1lido.")]
    public TipoAcessoEnum TipoAcesso { get; set; } = TipoAcessoEnum.Funcionario;

    public string? ReturnUrl { get; set; }
}
