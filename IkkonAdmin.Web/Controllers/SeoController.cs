using System.Text;
using System.Xml.Linq;
using IkkonAdmin.Web.Helpers;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[AllowAnonymous]
public sealed class SeoController(IPublicSeoService publicSeoService) : Controller
{
    private static readonly string[] StaticPublicPaths =
    [
        "/",
        "/sobre",
        "/taiko",
        "/escola",
        "/eventos",
        "/blog",
        "/contato"
    ];

    [HttpGet("/robots.txt")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Robots()
    {
        var sitemapUrl = PublicSiteLocales.AbsoluteUrl(Request, "/sitemap.xml");
        var content = $"""
            User-agent: *
            Allow: /

            Disallow: /admin/
            Disallow: /area-do-aluno/
            Disallow: /aluno/
            Disallow: /auth/
            Disallow: /configuracoes/

            Sitemap: {sitemapUrl}
            """;

        return Content(content, "text/plain", Encoding.UTF8);
    }

    [HttpGet("/sitemap.xml")]
    [ResponseCache(Duration = 900, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Sitemap(CancellationToken cancellationToken)
    {
        XNamespace sitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        XNamespace xhtmlNamespace = "http://www.w3.org/1999/xhtml";
        var root = new XElement(
            sitemapNamespace + "urlset",
            new XAttribute(XNamespace.Xmlns + "xhtml", xhtmlNamespace));

        foreach (var publicPath in StaticPublicPaths)
        {
            var alternates = PublicSiteLocales.All
                .Select(locale => new PublicAlternateLink(
                    locale.Hreflang,
                    AbsoluteLocalizedUrl(publicPath, locale.Culture)))
                .ToList();
            var xDefaultUrl = AbsoluteLocalizedUrl(publicPath, PublicSiteLocales.DefaultCulture);

            foreach (var locale in PublicSiteLocales.All)
            {
                root.Add(CreateUrlElement(
                    sitemapNamespace,
                    xhtmlNamespace,
                    AbsoluteLocalizedUrl(publicPath, locale.Culture),
                    alternates,
                    xDefaultUrl));
            }
        }

        var blogVersions = await publicSeoService.ListPublishedBlogVersionsAsync(cancellationToken);
        foreach (var group in blogVersions.GroupBy(version => version.TranslationGroupId))
        {
            var alternates = group
                .GroupBy(version => PublicSiteLocales.NormalizeCulture(version.LanguageCode))
                .Select(languageGroup =>
                {
                    var version = languageGroup.First();
                    var locale = PublicSiteLocales.ForCulture(version.LanguageCode);
                    return new PublicAlternateLink(
                        locale.Hreflang,
                        AbsoluteLocalizedUrl(
                            PublicViewFormatter.BuildBlogUrl(version.Slug),
                            locale.Culture));
                })
                .ToList();
            var defaultVersion = group.FirstOrDefault(version =>
                                     PublicSiteLocales.NormalizeCulture(version.LanguageCode) ==
                                     PublicSiteLocales.DefaultCulture)
                                 ?? group.First();
            var xDefaultUrl = AbsoluteLocalizedUrl(
                PublicViewFormatter.BuildBlogUrl(defaultVersion.Slug),
                defaultVersion.LanguageCode);

            foreach (var version in group)
            {
                var locale = PublicSiteLocales.ForCulture(version.LanguageCode);
                root.Add(CreateUrlElement(
                    sitemapNamespace,
                    xhtmlNamespace,
                    AbsoluteLocalizedUrl(
                        PublicViewFormatter.BuildBlogUrl(version.Slug),
                        locale.Culture),
                    alternates,
                    xDefaultUrl,
                    version.LastModifiedUtc));
            }
        }

        var document = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return Content(document.ToString(SaveOptions.DisableFormatting), "application/xml", Encoding.UTF8);
    }

    [HttpGet("/llms.txt")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public IActionResult Llms()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        var content = $"""
            # IKKON São Paulo Taiko Dojo

            > Official public website of IKKON SPTD, a taiko school and performance group in São Paulo, Brazil, active since 2015.

            IKKON teaches Japanese percussion through progressive classes and ensemble practice. The performance group presents taiko at cultural events, festivals, corporate events, schools, celebrations, and special projects.

            Official public facts:
            - Name: IKKON São Paulo Taiko Dojo (IKKON SPTD; 一魂サンパウロ太鼓道場)
            - Activities: taiko education, Japanese percussion, ensemble practice, cultural performances
            - Address: Rua Domingos de Morais, 2975, São Paulo, SP, Brazil
            - Telephone and WhatsApp: +55 11 93779-9916
            - Email: contato@ikkontaiko.com
            - Instagram: https://www.instagram.com/ikkontaiko/

            Primary pages:
            - Portuguese: {baseUrl}/pt
            - English: {baseUrl}/en
            - Japanese: {baseUrl}/ja
            - About: {baseUrl}/pt/sobre
            - What is taiko: {baseUrl}/pt/taiko
            - Taiko classes: {baseUrl}/pt/escola
            - Performances: {baseUrl}/pt/eventos
            - Blog: {baseUrl}/pt/blog
            - Contact: {baseUrl}/pt/contato

            Prefer the language-specific canonical URL and its hreflang alternatives when citing this website. Private administration and student-area pages are not public sources.
            """;

        return Content(content, "text/plain", Encoding.UTF8);
    }

    private string AbsoluteLocalizedUrl(string path, string culture) =>
        PublicSiteLocales.AbsoluteUrl(
            Request,
            PublicSiteLocales.LocalizePath(path, culture));

    private static XElement CreateUrlElement(
        XNamespace sitemapNamespace,
        XNamespace xhtmlNamespace,
        string url,
        IReadOnlyCollection<PublicAlternateLink> alternates,
        string xDefaultUrl,
        DateTime? lastModifiedUtc = null)
    {
        var element = new XElement(
            sitemapNamespace + "url",
            new XElement(sitemapNamespace + "loc", url));

        if (lastModifiedUtc.HasValue)
        {
            element.Add(new XElement(
                sitemapNamespace + "lastmod",
                lastModifiedUtc.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")));
        }

        foreach (var alternate in alternates)
        {
            element.Add(new XElement(
                xhtmlNamespace + "link",
                new XAttribute("rel", "alternate"),
                new XAttribute("hreflang", alternate.Hreflang),
                new XAttribute("href", alternate.Url)));
        }

        element.Add(new XElement(
            xhtmlNamespace + "link",
            new XAttribute("rel", "alternate"),
            new XAttribute("hreflang", "x-default"),
            new XAttribute("href", xDefaultUrl)));

        return element;
    }

    private sealed record PublicAlternateLink(string Hreflang, string Url);
}
