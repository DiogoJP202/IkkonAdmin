namespace IkkonAdmin.Web.Models.Entities;

public class UsuarioPermissao
{
    public int UsuarioId { get; set; }
    public UsuarioSistema? Usuario { get; set; }

    public int PermissaoId { get; set; }
    public PermissaoSistema? Permissao { get; set; }

    public DateTime DataConcessaoUtc { get; set; } = DateTime.UtcNow;
}
