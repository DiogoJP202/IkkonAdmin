using System.Security.Claims;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IkkonAdmin.Web.Security;

public static class AuthClaimsFactory
{
    public static ClaimsPrincipal CriarPrincipal(AuthSession authSession)
    {
        if (authSession.Usuario is null)
        {
            throw new ArgumentException("Resultado de autenticação sem usuário.", nameof(authSession));
        }

        var usuario = authSession.Usuario;
        var rolePrincipal = AppRoles.FromTipoAcesso(usuario.TipoAcesso);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NomeExibicao),
            new(AppClaimTypes.TipoAcesso, usuario.TipoAcesso.ToString()),
            new(AppClaimTypes.TemaPreferencia, usuario.TemaPreferencia.ToString())
        };

        if (!string.IsNullOrWhiteSpace(usuario.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, usuario.Email));
        }

        if (!string.IsNullOrWhiteSpace(usuario.FotoPerfilUrl))
        {
            claims.Add(new Claim(AppClaimTypes.FotoPerfilUrl, usuario.FotoPerfilUrl));
        }

        if (usuario.AlunoId.HasValue)
        {
            claims.Add(new Claim(AppClaimTypes.AlunoId, usuario.AlunoId.Value.ToString()));
        }

        foreach (var role in authSession.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!authSession.Roles.Contains(rolePrincipal, StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, rolePrincipal));
        }

        foreach (var permissao in authSession.Permissoes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(AppClaimTypes.Permissao, permissao));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
