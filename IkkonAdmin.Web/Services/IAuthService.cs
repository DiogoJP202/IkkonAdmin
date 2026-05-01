using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IAuthService
{
    Task<AuthResult> AutenticarAsync(
        string loginOuEmail,
        string senha,
        TipoAcessoEnum tipoAcesso,
        string? enderecoIp = null,
        CancellationToken cancellationToken = default);
}

public sealed record AuthResult(
    bool Sucesso,
    UsuarioSistema? Usuario,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissoes)
{
    public static AuthResult Falha() => new(false, null, Array.Empty<string>(), Array.Empty<string>());

    public static AuthResult Ok(
        UsuarioSistema usuario,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissoes) =>
        new(true, usuario, roles, permissoes);
}
