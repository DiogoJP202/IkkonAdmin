using System.Security.Claims;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("admin/blog")]
[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.BlogView)]
public class BlogAdminController(IBlogService blogService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] BlogAdminFilterViewModel filtro, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Blog";
        var vm = await blogService.ListarAsync(filtro, cancellationToken);
        return View(vm);
    }

    [HttpGet("criar")]
    [Authorize(Policy = AuthorizationPolicies.BlogCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo post";
        var vm = await blogService.ObterFormCriacaoAsync(ObterUsuarioId(), cancellationToken);
        return View(vm);
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.BlogCreate)]
    public async Task<IActionResult> Create(BlogPostFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo post";

        if (!PodeExecutarAcao(model))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await RecarregarOpcoesAsync(model, cancellationToken);
            return View(model);
        }

        var result = await blogService.CriarAsync(model, ObterUsuarioId(), cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await RecarregarOpcoesAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Edit), new { id = result.EntityId });
    }

    [HttpGet("editar/{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.BlogEdit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar post";
        var vm = await blogService.ObterFormEdicaoAsync(id, cancellationToken);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost("editar/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.BlogEdit)]
    public async Task<IActionResult> Edit(int id, BlogPostFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar post";

        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!PodeExecutarAcao(model))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await RecarregarOpcoesAsync(model, cancellationToken);
            return View(model);
        }

        var result = await blogService.AtualizarAsync(id, model, ObterUsuarioId(), cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await RecarregarOpcoesAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet("preview/{id:int}")]
    public async Task<IActionResult> Preview(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Preview do post";
        var vm = await blogService.ObterPreviewAsync(id, cancellationToken);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpPost("excluir/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.BlogDelete)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await blogService.ExcluirAsync(id, ObterUsuarioId(), cancellationToken);
        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    private async Task RecarregarOpcoesAsync(BlogPostFormViewModel model, CancellationToken cancellationToken)
    {
        var source = model.Id.HasValue
            ? await blogService.ObterFormEdicaoAsync(model.Id.Value, cancellationToken)
            : await blogService.ObterFormCriacaoAsync(ObterUsuarioId(), cancellationToken);

        model.CategoryOptions = source?.CategoryOptions ?? [];
        model.AuthorOptions = source?.AuthorOptions ?? [];
        model.TagSuggestions = source?.TagSuggestions ?? [];
        model.CurrentCoverImageUrl ??= source?.CurrentCoverImageUrl;
        model.CurrentStatus = model.Id.HasValue ? source?.CurrentStatus ?? model.CurrentStatus : model.CurrentStatus;
        model.CreatedAtUtc = source?.CreatedAtUtc ?? model.CreatedAtUtc;
        model.UpdatedAtUtc = source?.UpdatedAtUtc ?? model.UpdatedAtUtc;
        model.PublishedAtUtc = source?.PublishedAtUtc ?? model.PublishedAtUtc;
    }

    private bool PodeExecutarAcao(BlogPostFormViewModel model)
    {
        var action = (model.SubmissionAction ?? "Draft").Trim().ToLowerInvariant();

        if ((model.IsFeatured || model.IsWeeklyHighlight) && !User.HasPermission(AppPermissions.BlogFeature))
        {
            return false;
        }

        return action switch
        {
            "publish" => User.HasPermission(AppPermissions.BlogPublish),
            "schedule" => User.HasPermission(AppPermissions.BlogPublish),
            "archive" => User.HasPermission(AppPermissions.BlogArchive),
            _ => true
        };
    }

    private int? ObterUsuarioId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
