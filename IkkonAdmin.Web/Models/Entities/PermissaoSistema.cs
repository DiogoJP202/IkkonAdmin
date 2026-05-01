namespace IkkonAdmin.Web.Models.Entities;

public class PermissaoSistema
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public bool Ativo { get; set; } = true;
    public bool IsSistema { get; set; } = true;
    public DateTime DataCriacaoUtc { get; set; } = DateTime.UtcNow;

    public ICollection<RolePermissao> RolePermissoes { get; set; } = new List<RolePermissao>();
    public ICollection<UsuarioPermissao> UsuarioPermissoes { get; set; } = new List<UsuarioPermissao>();
}
