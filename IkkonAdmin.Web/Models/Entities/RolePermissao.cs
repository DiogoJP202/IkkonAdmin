namespace IkkonAdmin.Web.Models.Entities;

public class RolePermissao
{
    public int RoleId { get; set; }
    public RoleSistema? Role { get; set; }

    public int PermissaoId { get; set; }
    public PermissaoSistema? Permissao { get; set; }

    public DateTime DataVinculoUtc { get; set; } = DateTime.UtcNow;
}
