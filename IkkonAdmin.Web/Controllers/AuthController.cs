using System.Security.Claims;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("auth")]
public class AuthController(IAuthService authService) : Controller
{
    [AllowAnonymous]
    [HttpGet("")]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var roleAtual = ObterRolePrincipalDoUsuario(User);
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl) &&
                roleAtual is not null &&
                ReturnUrlCombinaComPerfil(returnUrl, roleAtual))
            {
                return Redirect(returnUrl);
            }

            return Redirect(ObterUrlPadraoParaUsuarioAutenticado(User));
        }

        ViewData["Title"] = "Entrar";
        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl,
            TipoAcesso = TipoAcessoEnum.Funcionario
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Entrar";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var authResult = await authService.AutenticarAsync(
            model.LoginOuEmail,
            model.Senha,
            model.TipoAcesso,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        if (!authResult.Sucesso || authResult.Usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Credenciais inv\u00E1lidas para o tipo de acesso selecionado.");
            return View(model);
        }

        var rolePrincipal = AppRoles.FromTipoAcesso(authResult.Usuario.TipoAcesso);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, authResult.Usuario.Id.ToString()),
            new(ClaimTypes.Name, authResult.Usuario.NomeExibicao),
            new(AppClaimTypes.TipoAcesso, authResult.Usuario.TipoAcesso.ToString())
        };

        if (!string.IsNullOrWhiteSpace(authResult.Usuario.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, authResult.Usuario.Email));
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

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        if (Url.IsLocalUrl(model.ReturnUrl) && ReturnUrlCombinaComPerfil(model.ReturnUrl!, rolePrincipal))
        {
            return Redirect(model.ReturnUrl!);
        }

        return Redirect(ObterUrlPadraoPorPerfil(rolePrincipal));
    }

    [Authorize]
    [HttpGet("acesso-negado")]
    public IActionResult AcessoNegado()
    {
        ViewData["Title"] = "Acesso negado";
        return View();
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Institucional");
    }

    private static bool ReturnUrlCombinaComPerfil(string returnUrl, string role)
    {
        if (returnUrl.StartsWith("/configuracoes", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (role == AppRoles.Admin)
        {
            return returnUrl.StartsWith("/admin", StringComparison.OrdinalIgnoreCase);
        }

        if (role == AppRoles.Funcionario)
        {
            return returnUrl.StartsWith("/admin", StringComparison.OrdinalIgnoreCase);
        }

        if (role == AppRoles.Aluno)
        {
            return returnUrl.StartsWith("/aluno", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static string ObterUrlPadraoPorPerfil(string? role)
    {
        return role switch
        {
            AppRoles.Admin => "/admin/painel",
            AppRoles.Funcionario => "/admin",
            AppRoles.Aluno => "/aluno",
            _ => "/auth/login"
        };
    }

    private static string ObterUrlPadraoParaUsuarioAutenticado(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(AppRoles.Admin))
        {
            return ObterUrlPadraoPorPerfil(AppRoles.Admin);
        }

        if (principal.IsInRole(AppRoles.Funcionario))
        {
            return ObterUrlPadraoPorPerfil(AppRoles.Funcionario);
        }

        if (principal.IsInRole(AppRoles.Aluno))
        {
            return ObterUrlPadraoPorPerfil(AppRoles.Aluno);
        }

        return "/auth/login";
    }

    private static string? ObterRolePrincipalDoUsuario(ClaimsPrincipal principal)
    {
        if (principal.IsInRole(AppRoles.Admin))
        {
            return AppRoles.Admin;
        }

        if (principal.IsInRole(AppRoles.Funcionario))
        {
            return AppRoles.Funcionario;
        }

        if (principal.IsInRole(AppRoles.Aluno))
        {
            return AppRoles.Aluno;
        }

        return null;
    }
}
