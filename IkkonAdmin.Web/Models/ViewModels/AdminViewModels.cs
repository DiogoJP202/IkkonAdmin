using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class AdminPainelViewModel
{
    public int TotalUsuarios { get; set; }
    public int UsuariosAtivos { get; set; }
    public int UsuariosAdmins { get; set; }
    public int UsuariosFuncionarios { get; set; }
    public int UsuariosAlunos { get; set; }
    public int TotalCargosAtivos { get; set; }
    public int LogsUltimas24h { get; set; }
    public List<AdminLogListItemViewModel> AtividadesRecentes { get; set; } = new();
}

public class AdminUsuariosIndexViewModel
{
    public string? Busca { get; set; }
    public TipoAcessoEnum? Tipo { get; set; }
    public bool? Ativo { get; set; }
    public bool IncluirExcluidos { get; set; }
    public int PaginaAtual { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public int TotalRegistros { get; set; }
    public List<AdminUsuarioListItemViewModel> Usuarios { get; set; } = new();
}

public class AdminUsuarioListItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Login { get; set; } = string.Empty;
    public TipoAcessoEnum TipoAcesso { get; set; }
    public int? RoleId { get; set; }
    public string? RoleNome { get; set; }
    public string? RoleCodigo { get; set; }
    public bool Ativo { get; set; }
    public bool Excluido { get; set; }
    public DateTime DataCriacaoUtc { get; set; }
    public DateTime? UltimoLoginUtc { get; set; }
}

public class AdminUsuarioFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Informe o nome do usuário.")]
    [StringLength(200, ErrorMessage = "Nome deve ter no máximo 200 caracteres.")]
    public string NomeExibicao { get; set; } = string.Empty;

    [Display(Name = "Login")]
    [Required(ErrorMessage = "Informe o login do usuário.")]
    [StringLength(80, ErrorMessage = "Login deve ter no máximo 80 caracteres.")]
    public string Login { get; set; } = string.Empty;

    [Display(Name = "E-mail")]
    [Required(ErrorMessage = "Informe o e-mail do usuário.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(150, ErrorMessage = "E-mail deve ter no máximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Telefone")]
    [StringLength(30)]
    public string? Telefone { get; set; }

    [Display(Name = "Tipo de conta")]
    [Required(ErrorMessage = "Selecione o tipo de conta.")]
    [EnumDataType(typeof(TipoAcessoEnum), ErrorMessage = "Tipo de conta inválido.")]
    public TipoAcessoEnum TipoAcesso { get; set; } = TipoAcessoEnum.Funcionario;

    [Display(Name = "Cargo")]
    [Required(ErrorMessage = "Selecione um cargo para o usuário.")]
    public int RoleId { get; set; }

    public List<AdminRoleSelectItemViewModel> RolesDisponiveis { get; set; } = new();

    [Display(Name = "Conta ativa")]
    public bool Ativo { get; set; } = true;

    [Display(Name = "Senha inicial")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Senha deve ter entre 8 e 100 caracteres.")]
    public string? SenhaInicial { get; set; }
}

public class AdminAcessosViewModel
{
    public int UsuarioId { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string EmailUsuario { get; set; } = string.Empty;
    public TipoAcessoEnum TipoAcesso { get; set; }
    public int RoleSelecionadaId { get; set; }
    public List<AdminRoleSelectItemViewModel> RolesDisponiveis { get; set; } = new();
    public List<AdminPermissaoSelectItemViewModel> PermissoesDisponiveis { get; set; } = new();
}

public class AdminRoleSelectItemViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public TipoAcessoEnum TipoAcesso { get; set; }
    public bool Ativo { get; set; }
    public bool IsSistema { get; set; }
}

public class AdminPermissaoSelectItemViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Modulo { get; set; }
    public bool Concedida { get; set; }
    public bool HerdadaDaRole { get; set; }
}

public class AdminAcessosUpdateRequest
{
    [Required]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "Selecione um cargo.")]
    public int RoleId { get; set; }

    public List<int> PermissoesDiretas { get; set; } = new();
}

public class AdminRolesIndexViewModel
{
    public string? Busca { get; set; }
    public TipoAcessoEnum? Tipo { get; set; }
    public bool? Ativo { get; set; }
    public int PaginaAtual { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public int TotalRegistros { get; set; }
    public List<AdminRoleListItemViewModel> Roles { get; set; } = new();
}

public class AdminRoleListItemViewModel
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoAcessoEnum TipoAcesso { get; set; }
    public bool Ativo { get; set; }
    public bool IsSistema { get; set; }
    public int TotalUsuarios { get; set; }
    public int TotalPermissoes { get; set; }
    public DateTime DataCriacaoUtc { get; set; }
}

public class AdminRoleFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Nome do cargo")]
    [Required(ErrorMessage = "Informe o nome do cargo.")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "Código interno")]
    [StringLength(60, ErrorMessage = "Código deve ter no máximo 60 caracteres.")]
    public string? Codigo { get; set; }

    [Display(Name = "Descrição")]
    [StringLength(300, ErrorMessage = "Descrição deve ter no máximo 300 caracteres.")]
    public string? Descricao { get; set; }

    [Display(Name = "Tipo de conta")]
    [Required(ErrorMessage = "Selecione o tipo de conta do cargo.")]
    [EnumDataType(typeof(TipoAcessoEnum), ErrorMessage = "Tipo de conta inválido.")]
    public TipoAcessoEnum TipoAcesso { get; set; } = TipoAcessoEnum.Funcionario;

    [Display(Name = "Cargo ativo")]
    public bool Ativo { get; set; } = true;

    public bool IsSistema { get; set; }

    public List<int> PermissoesSelecionadas { get; set; } = new();
    public List<AdminPermissaoSelectItemViewModel> PermissoesDisponiveis { get; set; } = new();
}

public class AdminLogsIndexViewModel
{
    public string? Busca { get; set; }
    public int? UsuarioResponsavelId { get; set; }
    public int PaginaAtual { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 20;
    public int TotalRegistros { get; set; }
    public List<AdminLogListItemViewModel> Logs { get; set; } = new();
    public List<AdminFiltroUsuarioViewModel> Responsaveis { get; set; } = new();
}

public class AdminFiltroUsuarioViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class AdminLogListItemViewModel
{
    public long Id { get; set; }
    public DateTime DataEventoUtc { get; set; }
    public string Acao { get; set; } = string.Empty;
    public string Entidade { get; set; } = string.Empty;
    public string? EntidadeId { get; set; }
    public string? Descricao { get; set; }
    public string? ResponsavelNome { get; set; }
    public string? AfetadoNome { get; set; }
}

