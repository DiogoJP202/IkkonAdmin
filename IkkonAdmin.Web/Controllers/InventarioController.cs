using System.Security.Claims;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("admin/inventario")]
[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.InventarioView)]
public class InventarioController(
    IInventarioQueryService inventarioQueryService,
    IInventarioService inventarioService,
    ICurrentUserService currentUserService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] InventarioFiltroViewModel filtro, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Inventário";
        var vm = await inventarioQueryService.ListarAsync(filtro, cancellationToken);
        return View(vm);
    }

    [HttpGet("detalhes/{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Detalhes do item";
        var vm = await inventarioQueryService.ObterDetalhesAsync(id, cancellationToken);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpGet("criar")]
    [Authorize(Policy = AuthorizationPolicies.InventarioCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo item";
        var vm = await inventarioQueryService.ObterFormCriacaoAsync(cancellationToken);
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
            var form = await inventarioQueryService.ObterFormCriacaoAsync(cancellationToken);
            model.TiposSugeridos = form.TiposSugeridos;
            return View(model);
        }

        var result = await inventarioService.CriarAsync(model, ObterUsuarioId(), cancellationToken);
        if (!result.Success)
        {
            result.AddToModelState(ModelState);
            var form = await inventarioQueryService.ObterFormCriacaoAsync(cancellationToken);
            model.TiposSugeridos = form.TiposSugeridos;
            return View(model);
        }

        result.AddToTempData(TempData);
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet("editar/{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.InventarioEdit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar item";
        var vm = await inventarioQueryService.ObterFormEdicaoAsync(id, cancellationToken);
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
            var form = await inventarioQueryService.ObterFormEdicaoAsync(id, cancellationToken);
            model.TiposSugeridos = form?.TiposSugeridos ?? [];
            return View(model);
        }

        var result = await inventarioService.AtualizarAsync(id, model, ObterUsuarioId(), cancellationToken);
        if (result.Status == OperationResultStatus.NotFound)
        {
            return NotFound();
        }

        if (!result.Success)
        {
            result.AddToModelState(ModelState);
            var form = await inventarioQueryService.ObterFormEdicaoAsync(id, cancellationToken);
            model.TiposSugeridos = form?.TiposSugeridos ?? [];
            return View(model);
        }

        result.AddToTempData(TempData);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.InventarioDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await inventarioService.InativarAsync(id, ObterUsuarioId(), cancellationToken);
        result.AddToTempData(TempData);
        return RedirectToAction(nameof(Index));
    }

    private int? ObterUsuarioId()
    {
        return currentUserService.UserId;
    }
}
