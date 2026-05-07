using System.Security.Claims;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("admin/inventario")]
[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.InventarioView)]
public class InventarioController(IInventarioService inventarioService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] InventarioFiltroViewModel filtro, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Inventário";
        var vm = await inventarioService.ListarAsync(filtro, cancellationToken);
        return View(vm);
    }

    [HttpGet("detalhes/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Detalhes do item";
        var vm = await inventarioService.ObterDetalhesAsync(id, cancellationToken);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpGet("criar")]
    [Authorize(Policy = AuthorizationPolicies.InventarioCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo item";
        var vm = await inventarioService.ObterFormCriacaoAsync(cancellationToken);
        return View(vm);
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.InventarioCreate)]
    public async Task<IActionResult> Create(InventarioFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo item";

        if (!ModelState.IsValid)
        {
            var form = await inventarioService.ObterFormCriacaoAsync(cancellationToken);
            model.TiposSugeridos = form.TiposSugeridos;
            return View(model);
        }

        var result = await inventarioService.CriarAsync(model, ObterUsuarioId(), cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var form = await inventarioService.ObterFormCriacaoAsync(cancellationToken);
            model.TiposSugeridos = form.TiposSugeridos;
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = result.EntityId });
    }

    [HttpGet("editar/{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.InventarioEdit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar item";
        var vm = await inventarioService.ObterFormEdicaoAsync(id, cancellationToken);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost("editar/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.InventarioEdit)]
    public async Task<IActionResult> Edit(int id, InventarioFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar item";

        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var form = await inventarioService.ObterFormEdicaoAsync(id, cancellationToken);
            model.TiposSugeridos = form?.TiposSugeridos ?? [];
            return View(model);
        }

        var result = await inventarioService.AtualizarAsync(id, model, ObterUsuarioId(), cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var form = await inventarioService.ObterFormEdicaoAsync(id, cancellationToken);
            model.TiposSugeridos = form?.TiposSugeridos ?? [];
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.InventarioDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await inventarioService.InativarAsync(id, ObterUsuarioId(), cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private int? ObterUsuarioId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
