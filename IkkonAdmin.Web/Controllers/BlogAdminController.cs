using System.Security.Claims;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("admin/blog")]
[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.BlogView)]
public class BlogAdminController(
    IBlogAdminQueryService blogAdminQueryService,
    IBlogService blogService,
    IBlogMediaService blogMediaService,
    ICurrentUserService currentUserService,
    IBlogPostActionAuthorizer blogPostActionAuthorizer) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] BlogAdminFilterViewModel filtro, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Blog";
        var vm = await blogAdminQueryService.ListarAsync(filtro, cancellationToken);
        return View(vm);
    }

    [HttpGet("criar")]
    [Authorize(Policy = AuthorizationPolicies.BlogCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo post";
        var vm = await blogAdminQueryService.ObterFormCriacaoAsync(ObterUsuarioId(), cancellationToken);
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
        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }

    [HttpGet("editar/{id:int}")]
    [Authorize(Policy = AuthorizationPolicies.BlogEdit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar post";
        var vm = await blogAdminQueryService.ObterFormEdicaoAsync(id, cancellationToken);
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
        var vm = await blogAdminQueryService.ObterPreviewAsync(id, cancellationToken);
        return vm is null ? NotFound() : View(vm);
    }

    [HttpGet("{id:int}/versoes")]
    [Authorize(Policy = AuthorizationPolicies.BlogEdit)]
    public async Task<IActionResult> Versions(int id, CancellationToken cancellationToken)
    {
        var vm = await blogAdminQueryService.ObterVersoesAsync(id, cancellationToken);
        return vm is null ? NotFound() : PartialView("_BlogPostVersionsModalBody", vm);
    }

    [HttpPost("{id:int}/versoes/criar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.BlogCreate)]
    public async Task<IActionResult> CreateVersion(int id, [FromForm] string languageCode, CancellationToken cancellationToken)
    {
        var result = await blogService.CriarVersaoAsync(id, languageCode, ObterUsuarioId(), cancellationToken);
        return Json(new
        {
            success = result.Success,
            message = result.Message,
            entityId = result.Value,
            redirectUrl = result.Success
                ? Url.Action(nameof(Edit), new { id = result.Value })
                : null
        });
    }

    [HttpPost("{id:int}/versoes/excluir/{versionId:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.BlogDelete)]
    public async Task<IActionResult> DeleteVersion(int id, int versionId, CancellationToken cancellationToken)
    {
        var result = await blogService.ExcluirVersaoAsync(id, versionId, ObterUsuarioId(), cancellationToken);
        return Json(new
        {
            success = result.Success,
            message = result.Message,
            entityId = result.Value
        });
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

    [HttpPost("midia/imagem-conteudo")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadContentImage(IFormFile? image, CancellationToken cancellationToken)
    {
        if (!blogPostActionAuthorizer.CanUploadContentImage(User))
        {
            return Forbid();
        }

        if (image is null)
        {
            return BadRequest(new { success = false, message = "Selecione uma imagem válida." });
        }

        var result = await blogMediaService.SaveContentImageAsync(image, cancellationToken);
        if (!result.Success || string.IsNullOrWhiteSpace(result.PublicUrl))
        {
            return BadRequest(new { success = false, message = result.Message });
        }

        return Json(new { success = true, url = result.PublicUrl });
    }

    private async Task RecarregarOpcoesAsync(BlogPostFormViewModel model, CancellationToken cancellationToken)
    {
        var source = model.Id.HasValue
            ? await blogAdminQueryService.ObterFormEdicaoAsync(model.Id.Value, cancellationToken)
            : await blogAdminQueryService.ObterFormCriacaoAsync(ObterUsuarioId(), cancellationToken);

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
        return blogPostActionAuthorizer.CanSubmit(User, model);
    }

    private int? ObterUsuarioId()
    {
        return currentUserService.UserId;
    }
}
