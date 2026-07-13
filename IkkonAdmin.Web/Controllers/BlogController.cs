using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[AllowAnonymous]
[Route("blog")]
public class BlogController(IBlogService blogService, IViewTextService i18n) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] BlogPublicFilterViewModel filtro,
        CancellationToken cancellationToken)
    {
        ViewData["Title"] = i18n["Blog | IKKON SPTD", "Blog | IKKON SPTD", "ブログ | IKKON SPTD"];
        ViewData["Description"] = i18n[
            "Conteúdos, novidades e bastidores do IKKON SPTD, escola de taiko em São Paulo.",
            "Content, news, and behind the scenes from IKKON SPTD, a taiko school in Sao Paulo.",
            "サンパウロの和太鼓教室IKKON SPTDの読みもの、ニュース、舞台裏をお届けします。"];
        ViewData["CanonicalUrl"] = Url.Action(nameof(Index), "Blog", values: null, protocol: Request.Scheme);
        ViewData["OgType"] = "website";
        ViewData["PublicSection"] = "blog";
        ViewData["JapanesePublicEnabled"] = true;

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
            : viewModel.Summary ?? i18n[
                "Conteúdo do Blog IKKON SPTD sobre taiko, cultura japonesa e comunidade.",
                "IKKON SPTD blog content about taiko, Japanese culture, and community.",
                "太鼓、日本文化、コミュニティに関するIKKON SPTDブログの記事です。"];

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
        ViewData["JapanesePublicEnabled"] = true;

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
