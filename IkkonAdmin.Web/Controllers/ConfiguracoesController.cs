using System.Security.Claims;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IkkonAdmin.Web.Services;
using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IkkonAdmin.Web.Controllers;

[Authorize]
[Authorize(Policy = AuthorizationPolicies.ConfiguracoesView)]
public class ConfiguracoesController(
    IUserSettingsQueryService userSettingsQueryService,
    IUserSettingsService userSettingsService,
    IAuthService authService,
    ICurrentUserService currentUserService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Configurações";

        if (!TryGetCurrentUserId(out var userId))
        {
            return Challenge();
        }

        var vm = await userSettingsQueryService.GetPageAsync(userId, cancellationToken);
        if (vm is null)
        {
            return Forbid();
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConfiguracoesEdit)]
    public async Task<IActionResult> AtualizarConta([FromForm] UpdateAccountInfoRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { success = false, message = "Sessão inválida. Faça login novamente." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Revise os campos destacados e tente novamente.",
                errors = BuildModelErrors()
            });
        }

        var result = await userSettingsService.UpdateAccountInfoAsync(userId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        if (!await RenovarSessaoAsync(userId, cancellationToken))
        {
            return Unauthorized(new { success = false, message = "Sessão inválida. Faça login novamente." });
        }

        return Ok(new { success = true, message = result.Message, refreshPage = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConfiguracoesEdit)]
    public async Task<IActionResult> AlterarSenha([FromForm] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { success = false, message = "Sessão inválida. Faça login novamente." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Revise os campos destacados e tente novamente.",
                errors = BuildModelErrors()
            });
        }

        var result = await userSettingsService.ChangePasswordAsync(userId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        return Ok(new { success = true, message = result.Message });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConfiguracoesEdit)]
    public async Task<IActionResult> AtualizarPreferencias([FromForm] UpdatePreferencesRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized(new { success = false, message = "Sessão inválida. Faça login novamente." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Revise os campos destacados e tente novamente.",
                errors = BuildModelErrors()
            });
        }

        var result = await userSettingsService.UpdatePreferencesAsync(userId, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        if (!await RenovarSessaoAsync(userId, cancellationToken))
        {
            return Unauthorized(new { success = false, message = "Sessão inválida. Faça login novamente." });
        }

        return Ok(new { success = true, message = result.Message, refreshPage = true });
    }

    private async Task<bool> RenovarSessaoAsync(int userId, CancellationToken cancellationToken)
    {
        var sessionResult = await authService.RecarregarSessaoAsync(userId, cancellationToken);
        if (!sessionResult.Success || sessionResult.Value is null)
        {
            return false;
        }

        var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = authenticateResult.Properties ?? new AuthenticationProperties
        {
            IsPersistent = false,
            AllowRefresh = true
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthClaimsFactory.CriarPrincipal(sessionResult.Value),
            properties);

        return true;
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        if (currentUserService.UserId is int currentUserId)
        {
            userId = currentUserId;
            return true;
        }

        userId = 0;
        return false;
    }

    private Dictionary<string, string[]> BuildModelErrors()
    {
        return ModelState
            .Where(x => x.Value is not null && x.Value.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage).ToArray());
    }
}
