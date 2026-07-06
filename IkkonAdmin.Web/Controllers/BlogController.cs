using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[AllowAnonymous]
[Route("blog")]
public class BlogController(IBlogService blogService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] BlogPublicFilterViewModel filtro,
        CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Blog | IKKON SPTD";
        ViewData["Description"] = "Conteudos, novidades e bastidores do IKKON SPTD, escola de taiko em Sao Paulo.";
        ViewData["CanonicalUrl"] = Url.Action(nameof(Index), "Blog", values: null, protocol: Request.Scheme);
        ViewData["OgType"] = "website";
        ViewData["PublicSection"] = "blog";

        var viewModel = await blogService.ListarPublicoAsync(filtro, cancellationToken);
        return View(viewModel);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var viewModel = await blogService.ObterPublicoPorSlugAsync(slug, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        var title = !string.IsNullOrWhiteSpace(viewModel.SeoTitle)
            ? viewModel.SeoTitle
            : $"{viewModel.Title} | Blog IKKON SPTD";
        var description = !string.IsNullOrWhiteSpace(viewModel.SeoDescription)
            ? viewModel.SeoDescription
            : viewModel.Summary ?? "Conteudo do Blog IKKON SPTD sobre taiko, cultura japonesa e comunidade.";

        ViewData["Title"] = title;
        ViewData["Description"] = description;
        ViewData["OgTitle"] = title;
        ViewData["OgDescription"] = description;
        ViewData["OgType"] = "article";
        ViewData["CanonicalUrl"] = Url.Action(nameof(Details), "Blog", new { slug = viewModel.Slug }, Request.Scheme);
        ViewData["OgImage"] = ToAbsolutePublicUrl(viewModel.CoverImageUrl)
                              ?? ToAbsolutePublicUrl(Url.Content("~/Images/Ikkon_Icon.png"));
        ViewData["PublicSection"] = "blog";
        ViewData["ContactMode"] = "geral";

        return View(viewModel);
    }

    private string? ToAbsolutePublicUrl(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return null;
        }

        if (Uri.TryCreate(publicUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        if (publicUrl.StartsWith("/", StringComparison.Ordinal))
        {
            return $"{Request.Scheme}://{Request.Host}{publicUrl}";
        }

        return $"{Request.Scheme}://{Request.Host}/{publicUrl.TrimStart('~', '/')}";
    }
}
