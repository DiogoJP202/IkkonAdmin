using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("admin/blog/categorias")]
[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.BlogCategoryManage)]
public class BlogCategoriasController(IBlogCategoriaService blogCategoriaService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Categorias do blog";
        var vm = await blogCategoriaService.ListarAsync(cancellationToken);
        return View(vm);
    }

    [HttpGet("criar")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Nova categoria";
        var vm = await blogCategoriaService.ObterParaCriacaoAsync(cancellationToken);
        return View(vm);
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogCategoryFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Nova categoria";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await blogCategoriaService.CriarAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("editar/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar categoria";
        var vm = await blogCategoriaService.ObterParaEdicaoAsync(id, cancellationToken);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost("editar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogCategoryFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar categoria";

        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await blogCategoriaService.AtualizarAsync(id, model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("status/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id, [FromForm] bool ativo, CancellationToken cancellationToken)
    {
        var result = await blogCategoriaService.AlterarStatusAsync(id, ativo, cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
