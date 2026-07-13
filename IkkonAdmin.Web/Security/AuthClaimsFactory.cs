using System.Security.Claims;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IkkonAdmin.Web.Security;

public static class AuthClaimsFactory
{
    public static ClaimsPrincipal CriarPrincipal(AuthResult authResult)
    {
        if (authResult.Usuario is null)
        {
            throw new ArgumentException("Resultado de autenticação sem usuário.", nameof(authResult));
        }

        var usuario = authResult.Usuario;
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

        foreach (var role in authResult.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (!authResult.Roles.Contains(rolePrincipal, StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(ClaimTypes.Role, rolePrincipal));
        }

        foreach (var permissao in authResult.Permissoes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(AppClaimTypes.Permissao, permissao));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
