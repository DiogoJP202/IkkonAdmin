namespace IkkonAdmin.Web.Models.Entities;

public class UsuarioRole
{
    public int UsuarioId { get; set; }
    public UsuarioSistema? Usuario { get; set; }

    public int RoleId { get; set; }
    public RoleSistema? Role { get; set; }

    public DateTime DataVinculoUtc { get; set; } = DateTime.UtcNow;
}
