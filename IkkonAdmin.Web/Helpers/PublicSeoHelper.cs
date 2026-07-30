using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Helpers;

public static class PublicSeoHelper
{
    public const string OrganizationName = "IKKON São Paulo Taiko Dojo";
    public const string ShortOrganizationName = "IKKON SPTD";
    public const string Email = "contato@ikkontaiko.com";
    public const string Telephone = "+55 11 93779-9916";
    public const string InstagramUrl = "https://www.instagram.com/ikkontaiko/";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string Organization(HttpRequest request)
    {
        var homeUrl = PublicSiteLocales.AbsoluteUrl(request, "/pt");

        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = new[] { "EducationalOrganization", "LocalBusiness" },
            ["@id"] = $"{homeUrl}#organization",
            ["name"] = OrganizationName,
            ["alternateName"] = new[]
            {
                ShortOrganizationName,
                "一魂サンパウロ太鼓道場"
            },
            ["url"] = homeUrl,
            ["logo"] = PublicSiteLocales.AbsoluteUrl(request, "/Images/Ikkon_Icon.png"),
            ["image"] = PublicSiteLocales.AbsoluteUrl(
                request,
                "/design-ikkon/social/ikkon-social-preview.jpg"),
            ["foundingDate"] = "2015",
            ["email"] = Email,
            ["telephone"] = Telephone,
            ["address"] = new Dictionary<string, object?>
            {
                ["@type"] = "PostalAddress",
                ["streetAddress"] = "Rua Domingos de Morais, 2975",
                ["addressLocality"] = "São Paulo",
                ["addressRegion"] = "SP",
                ["addressCountry"] = "BR"
            },
            ["areaServed"] = new Dictionary<string, object?>
            {
                ["@type"] = "City",
                ["name"] = "São Paulo"
            },
            ["sameAs"] = new[] { InstagramUrl }
        });
    }

    public static string WebSite(HttpRequest request)
    {
        var homeUrl = PublicSiteLocales.AbsoluteUrl(request, "/pt");

        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "WebSite",
            ["@id"] = $"{homeUrl}#website",
            ["url"] = homeUrl,
            ["name"] = OrganizationName,
            ["alternateName"] = ShortOrganizationName,
            ["inLanguage"] = PublicSiteLocales.All.Select(locale => locale.Hreflang).ToArray(),
            ["publisher"] = new Dictionary<string, object?>
            {
                ["@id"] = $"{homeUrl}#organization"
            }
        });
    }

    public static string WebPage(
        HttpRequest request,
        string canonicalUrl,
        string title,
        string description,
        string language,
        string schemaType = "WebPage")
    {
        var homeUrl = PublicSiteLocales.AbsoluteUrl(request, "/pt");

        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = schemaType,
            ["@id"] = $"{canonicalUrl}#webpage",
            ["url"] = canonicalUrl,
            ["name"] = title,
            ["description"] = description,
            ["inLanguage"] = language,
            ["isPartOf"] = new Dictionary<string, object?>
            {
                ["@id"] = $"{homeUrl}#website"
            },
            ["about"] = new Dictionary<string, object?>
            {
                ["@id"] = $"{homeUrl}#organization"
            }
        });
    }

    public static string Breadcrumbs(
        IEnumerable<(string Label, string Url)> items)
    {
        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BreadcrumbList",
            ["itemListElement"] = items
                .Select((item, index) => new Dictionary<string, object?>
                {
                    ["@type"] = "ListItem",
                    ["position"] = index + 1,
                    ["name"] = item.Label,
                    ["item"] = item.Url
                })
                .ToArray()
        });
    }

    public static string FaqPage(IEnumerable<(string Question, string Answer)> items)
    {
        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "FAQPage",
            ["mainEntity"] = items.Select(item => new Dictionary<string, object?>
            {
                ["@type"] = "Question",
                ["name"] = item.Question,
                ["acceptedAnswer"] = new Dictionary<string, object?>
                {
                    ["@type"] = "Answer",
                    ["text"] = item.Answer
                }
            }).ToArray()
        });
    }

    public static string Courses(
        HttpRequest request,
        string schoolUrl,
        IEnumerable<(string Name, string Description)> courses)
    {
        var homeUrl = PublicSiteLocales.AbsoluteUrl(request, "/pt");

        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = courses.Select((course, index) => new Dictionary<string, object?>
            {
                ["@type"] = "Course",
                ["@id"] = $"{schoolUrl}#course-{index + 1}",
                ["name"] = course.Name,
                ["description"] = course.Description,
                ["url"] = $"{schoolUrl}#cursos",
                ["provider"] = new Dictionary<string, object?>
                {
                    ["@id"] = $"{homeUrl}#organization"
                }
            }).ToArray()
        });
    }

    public static string MusicGroup(HttpRequest request, string performancesUrl)
    {
        var homeUrl = PublicSiteLocales.AbsoluteUrl(request, "/pt");

        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "MusicGroup",
            ["@id"] = $"{performancesUrl}#ensemble",
            ["name"] = "IKKON Taiko Arts Ensemble",
            ["url"] = performancesUrl,
            ["genre"] = new[] { "Taiko", "Wadaiko", "Japanese percussion" },
            ["location"] = new Dictionary<string, object?>
            {
                ["@type"] = "City",
                ["name"] = "São Paulo"
            },
            ["memberOf"] = new Dictionary<string, object?>
            {
                ["@id"] = $"{homeUrl}#organization"
            }
        });
    }

    public static string Article(
        HttpRequest request,
        string canonicalUrl,
        string title,
        string description,
        string language,
        DateTime publishedAtUtc,
        DateTime updatedAtUtc,
        string? authorName,
        string? imageUrl)
    {
        var homeUrl = PublicSiteLocales.AbsoluteUrl(request, "/pt");

        return Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "BlogPosting",
            ["@id"] = $"{canonicalUrl}#article",
            ["mainEntityOfPage"] = new Dictionary<string, object?>
            {
                ["@id"] = $"{canonicalUrl}#webpage"
            },
            ["url"] = canonicalUrl,
            ["headline"] = title,
            ["description"] = description,
            ["inLanguage"] = language,
            ["datePublished"] = publishedAtUtc.ToUniversalTime().ToString("O"),
            ["dateModified"] = updatedAtUtc.ToUniversalTime().ToString("O"),
            ["image"] = imageUrl,
            ["author"] = new Dictionary<string, object?>
            {
                ["@type"] = string.IsNullOrWhiteSpace(authorName) ? "Organization" : "Person",
                ["name"] = string.IsNullOrWhiteSpace(authorName) ? OrganizationName : authorName
            },
            ["publisher"] = new Dictionary<string, object?>
            {
                ["@id"] = $"{homeUrl}#organization"
            }
        });
    }

    private static string Serialize(object value) => JsonSerializer.Serialize(value, JsonOptions);
}
