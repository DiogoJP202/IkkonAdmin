using System.Security.Claims;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IkkonAdmin.Web.Services;
using IkkonAdmin.Web.Security;

namespace IkkonAdmin.Web.Controllers;

[Authorize]
[Authorize(Policy = AuthorizationPolicies.ConfiguracoesView)]
public class ConfiguracoesController(IUserSettingsService userSettingsService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Configurações";

        if (!TryGetCurrentUserId(out var userId))
        {
            return Challenge();
        }

        var vm = await userSettingsService.GetPageAsync(userId, cancellationToken);
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

        return Ok(new { success = true, message = result.Message });
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

        return Ok(new { success = true, message = result.Message });
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out userId);
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
