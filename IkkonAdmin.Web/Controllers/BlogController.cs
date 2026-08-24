using IkkonAdmin.Web.Helpers;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[AllowAnonymous]
[Route("blog")]
[Route("{culture:regex(^(pt|en|ja)$)}/blog")]
public class BlogController(IBlogPublicService blogPublicService, IViewTextService i18n) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] BlogPublicFilterViewModel filtro,
        CancellationToken cancellationToken)
    {
        var viewModel = await blogPublicService.ListarPublicoAsync(filtro, cancellationToken);
        var hasTaxonomyOrSearchFilter =
            !string.IsNullOrWhiteSpace(viewModel.Filtro.Q) ||
            !string.IsNullOrWhiteSpace(viewModel.Filtro.Categoria) ||
            !string.IsNullOrWhiteSpace(viewModel.Filtro.Tag);
        var title = viewModel.CurrentPage > 1
            ? i18n[
                $"Blog — página {viewModel.CurrentPage} | IKKON SPTD",
                $"Blog — page {viewModel.CurrentPage} | IKKON SPTD",
                $"ブログ — {viewModel.CurrentPage}ページ | IKKON SPTD"]
            : i18n["Blog | IKKON SPTD", "Blog | IKKON SPTD", "ブログ | IKKON SPTD"];
        var description = i18n[
            "Conteúdos, novidades e bastidores do IKKON SPTD sobre taiko, cultura japonesa, aulas, eventos e comunidade em São Paulo.",
            "Stories, news, and behind the scenes from IKKON SPTD about taiko, Japanese culture, classes, events, and community in Sao Paulo.",
            "サンパウロのIKKON SPTDから、和太鼓、日本文化、レッスン、イベント、コミュニティの読みものをお届けします。"];
        var paginationSuffix = !hasTaxonomyOrSearchFilter && viewModel.CurrentPage > 1
            ? $"?pagina={viewModel.CurrentPage}"
            : string.Empty;
        var canonicalPath = $"{i18n.LocalizePath("/blog")}{paginationSuffix}";
        var canonicalUrl = PublicSiteLocales.AbsoluteUrl(Request, canonicalPath);
        var locale = PublicSiteLocales.ForCulture(i18n.CurrentCulture);
        var homeLabel = i18n["Início", "Home", "ホーム"];
        var blogUrl = PublicSiteLocales.AbsoluteUrl(Request, i18n.LocalizePath("/blog"));

        ViewData["Title"] = title;
        ViewData["Description"] = description;
        ViewData["CanonicalPath"] = canonicalPath;
        ViewData["CanonicalUrl"] = canonicalUrl;
        ViewData["OgType"] = "website";
        ViewData["PublicSection"] = "blog";
        ViewData["JapanesePublicEnabled"] = true;
        ViewData["Robots"] = hasTaxonomyOrSearchFilter
            ? "noindex,follow,max-image-preview:large"
            : "index,follow,max-image-preview:large";
        ViewData["Breadcrumbs"] = new List<PublicBreadcrumbItemViewModel>
        {
            new(homeLabel, i18n.LocalizePath("/")),
            new("Blog")
        };
        ViewData["AlternateLinks"] = PublicSiteLocales.All
            .Select(alternateLocale => new PublicAlternateLinkViewModel(
                alternateLocale.Hreflang,
                PublicSiteLocales.AbsoluteUrl(
                    Request,
                    $"{PublicSiteLocales.LocalizePath("/blog", alternateLocale.Culture)}{paginationSuffix}")))
            .ToList();
        ViewData["StructuredData"] = new List<string>
        {
            PublicSeoHelper.WebPage(
                Request,
                canonicalUrl,
                title,
                description,
                locale.Hreflang,
                "CollectionPage"),
            PublicSeoHelper.Breadcrumbs(
            [
                (homeLabel, PublicSiteLocales.AbsoluteUrl(Request, i18n.LocalizePath("/"))),
                ("Blog", blogUrl)
            ])
        };

        return View(viewModel);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug, CancellationToken cancellationToken)
    {
        var viewModel = await blogPublicService.ObterPublicoPorSlugAsync(slug, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        var actualLocale = PublicSiteLocales.ForCulture(viewModel.LanguageCode);
        var routeCulture = RouteData.Values["culture"]?.ToString();
        if (!string.IsNullOrWhiteSpace(routeCulture) &&
            !routeCulture.Equals(actualLocale.Segment, StringComparison.OrdinalIgnoreCase))
        {
            return RedirectPermanent(PublicSiteLocales.LocalizePath(
                PublicViewFormatter.BuildBlogUrl(viewModel.Slug),
                actualLocale.Culture));
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
        var canonicalPath = PublicSiteLocales.LocalizePath(
            PublicViewFormatter.BuildBlogUrl(viewModel.Slug),
            actualLocale.Culture);
        var canonicalUrl = PublicSiteLocales.AbsoluteUrl(Request, canonicalPath);
        var imageUrl = ToAbsolutePublicUrl(viewModel.CoverImageUrl)
            ?? ToAbsolutePublicUrl(Url.Content("~/design-ikkon/social/ikkon-social-preview.jpg"));
        var alternateLinks = viewModel.AlternateVersions
            .GroupBy(version => PublicSiteLocales.NormalizeCulture(version.LanguageCode))
            .Select(group =>
            {
                var version = group.First();
                var locale = PublicSiteLocales.ForCulture(version.LanguageCode);
                return new PublicAlternateLinkViewModel(
                    locale.Hreflang,
                    PublicSiteLocales.AbsoluteUrl(
                        Request,
                        PublicSiteLocales.LocalizePath(
                            PublicViewFormatter.BuildBlogUrl(version.Slug),
                            locale.Culture)));
            })
            .ToList();
        var homeLabel = i18n["Início", "Home", "ホーム"];
        var blogUrl = PublicSiteLocales.AbsoluteUrl(
            Request,
            PublicSiteLocales.LocalizePath("/blog", actualLocale.Culture));

        ViewData["Title"] = title;
        ViewData["Description"] = description;
        ViewData["OgTitle"] = title;
        ViewData["OgDescription"] = description;
        ViewData["OgType"] = "article";
        ViewData["OgImage"] = imageUrl;
        ViewData["CanonicalPath"] = canonicalPath;
        ViewData["CanonicalUrl"] = canonicalUrl;
        ViewData["AlternateLinks"] = alternateLinks;
        ViewData["XDefaultUrl"] = alternateLinks
            .FirstOrDefault(link => link.Hreflang == "pt-BR")?.Url
            ?? canonicalUrl;
        ViewData["ArticlePublishedTime"] = viewModel.PublishedAtUtc.ToUniversalTime().ToString("O");
        ViewData["ArticleModifiedTime"] = viewModel.UpdatedAtUtc.ToUniversalTime().ToString("O");
        ViewData["PublicSection"] = "blog";
        ViewData["ContactMode"] = "geral";
        ViewData["JapanesePublicEnabled"] = true;
        ViewData["Breadcrumbs"] = new List<PublicBreadcrumbItemViewModel>
        {
            new(homeLabel, PublicSiteLocales.LocalizePath("/", actualLocale.Culture)),
            new("Blog", PublicSiteLocales.LocalizePath("/blog", actualLocale.Culture)),
            new(viewModel.Title)
        };
        ViewData["StructuredData"] = new List<string>
        {
            PublicSeoHelper.WebPage(
                Request,
                canonicalUrl,
                title,
                description,
                actualLocale.Hreflang,
                "Article"),
            PublicSeoHelper.Breadcrumbs(
            [
                (homeLabel, PublicSiteLocales.AbsoluteUrl(
                    Request,
                    PublicSiteLocales.LocalizePath("/", actualLocale.Culture))),
                ("Blog", blogUrl),
                (viewModel.Title, canonicalUrl)
            ]),
            PublicSeoHelper.Article(
                Request,
                canonicalUrl,
                viewModel.Title,
                description,
                actualLocale.Hreflang,
                viewModel.PublishedAtUtc,
                viewModel.UpdatedAtUtc,
                viewModel.AuthorName,
                imageUrl)
        };

        return View(viewModel);
    }

    private string? ToAbsolutePublicUrl(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return null;
        }

        // O caminho raiz-relativo é avaliado primeiro: no Linux ele também
        // satisfaz `UriKind.Absolute` e seria devolvido como URL file://.
        if (publicUrl.StartsWith("/", StringComparison.Ordinal))
        {
            return $"{Request.Scheme}://{Request.Host}{publicUrl}";
        }

        if (Uri.TryCreate(publicUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        return $"{Request.Scheme}://{Request.Host}/{publicUrl.TrimStart('~', '/')}";
    }
}
