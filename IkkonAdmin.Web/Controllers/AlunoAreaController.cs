using System.Security.Claims;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Aluno)]
public class AlunoAreaController(IAreaAlunoService areaAlunoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Área do Aluno";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterDashboardAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Perfil(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Meu Perfil";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterPerfilAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Financeiro(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Minhas Mensalidades";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterFinanceiroAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Turmas(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Minhas Turmas";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterTurmasAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public IActionResult AcessoIndisponivel()
    {
        ViewData["Title"] = "Acesso indisponível";
        return View();
    }

    private int? ObterUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var usuarioId) ? usuarioId : null;
    }
}
