using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("aluno")]
public class AlunoAuthController(IAuthService authService) : Controller
{
    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole(AppRoles.Aluno))
            {
                return Redirect("/area-do-aluno");
            }

            return Redirect(User.IsInRole(AppRoles.Admin) ? "/admin/painel" : "/admin");
        }

        ViewData["Title"] = "Entrar na Área do Aluno";
        return View(new LoginViewModel
        {
            TipoAcesso = TipoAcessoEnum.Aluno,
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Entrar na Área do Aluno";
        model.TipoAcesso = TipoAcessoEnum.Aluno;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var authResult = await authService.AutenticarAsync(
            model.LoginOuEmail,
            model.Senha,
            TipoAcessoEnum.Aluno,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        if (!authResult.Sucesso || authResult.Usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Credenciais inválidas para a Área do Aluno.");
            return View(model);
        }

        if (!authResult.Usuario.AlunoId.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Sua conta de aluno ainda não está vinculada a um cadastro ativo.");
            return View(model);
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthClaimsFactory.CriarPrincipal(authResult),
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });

        if (Url.IsLocalUrl(model.ReturnUrl) &&
            model.ReturnUrl!.StartsWith("/area-do-aluno", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(model.ReturnUrl);
        }

        return Redirect("/area-do-aluno");
    }

    [Authorize(Policy = AuthorizationPolicies.Aluno)]
    [HttpPost("sair")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
