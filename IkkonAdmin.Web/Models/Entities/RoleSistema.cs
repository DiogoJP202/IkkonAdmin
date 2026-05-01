using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class RoleSistema
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public TipoAcessoEnum TipoAcesso { get; set; } = TipoAcessoEnum.Funcionario;
    public bool Ativo { get; set; } = true;
    public bool IsSistema { get; set; } = true;
    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;

    public ICollection<UsuarioRole> UsuarioRoles { get; set; } = new List<UsuarioRole>();
    public ICollection<RolePermissao> RolePermissoes { get; set; } = new List<RolePermissao>();
}
