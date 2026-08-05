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

    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await blogCategoriaService.ExcluirAsync(id, cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("modal-data")]
    public async Task<IActionResult> ModalData(int? selectedCategoryId, CancellationToken cancellationToken)
    {
        return Json(await CriarPayloadModalAsync(selectedCategoryId, cancellationToken));
    }

    [HttpPost("modal/criar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModalCreate(BlogCategoryFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = ObterPrimeiroErroModelState() });
        }

        var result = await blogCategoriaService.CriarAsync(model, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        return Json(new
        {
            success = true,
            message = result.Message,
            entityId = result.Value,
            data = await CriarPayloadModalAsync(result.Value, cancellationToken)
        });
    }

    [HttpPost("modal/editar/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModalEdit(int id, BlogCategoryFormViewModel model, CancellationToken cancellationToken)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = ObterPrimeiroErroModelState() });
        }

        var result = await blogCategoriaService.AtualizarAsync(id, model, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        return Json(new
        {
            success = true,
            message = result.Message,
            entityId = result.Value,
            data = await CriarPayloadModalAsync(result.Value, cancellationToken)
        });
    }

    [HttpPost("modal/status/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModalToggleStatus(int id, [FromForm] bool ativo, [FromForm] int? selectedCategoryId, CancellationToken cancellationToken)
    {
        var result = await blogCategoriaService.AlterarStatusAsync(id, ativo, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        return Json(new
        {
            success = true,
            message = result.Message,
            entityId = result.Value,
            data = await CriarPayloadModalAsync(selectedCategoryId, cancellationToken)
        });
    }

    [HttpPost("modal/excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModalDelete(int id, [FromForm] int? selectedCategoryId, CancellationToken cancellationToken)
    {
        var result = await blogCategoriaService.ExcluirAsync(id, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        var nextSelectedCategoryId = selectedCategoryId == id ? null : selectedCategoryId;

        return Json(new
        {
            success = true,
            message = result.Message,
            entityId = result.Value,
            data = await CriarPayloadModalAsync(nextSelectedCategoryId, cancellationToken)
        });
    }

    private async Task<object> CriarPayloadModalAsync(int? selectedCategoryId, CancellationToken cancellationToken)
    {
        var index = await blogCategoriaService.ListarAsync(cancellationToken);
        var opcoes = await blogCategoriaService.ListarOpcoesAtivasAsync(selectedCategoryId, cancellationToken);

        return new
        {
            selectedCategoryId,
            categories = index.Categories.Select(x => new
            {
                id = x.Id,
                name = x.Name,
                slug = x.Slug,
                description = x.Description,
                isActive = x.IsActive,
                totalPosts = x.TotalPosts
            }),
            options = opcoes.Select(x => new
            {
                id = x.Id,
                name = x.Name,
                isActive = x.IsActive
            })
        };
    }

    private string ObterPrimeiroErroModelState()
    {
        return ModelState.Values
            .SelectMany(x => x.Errors)
            .Select(x => x.ErrorMessage)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ??
            "Revise os dados da categoria.";
    }
}
