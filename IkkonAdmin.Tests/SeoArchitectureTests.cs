using System.Text.Json;
using IkkonAdmin.Web.Helpers;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Tests;

public sealed class SeoArchitectureTests
{
    [Theory]
    [InlineData("/", "pt-BR", "/pt")]
    [InlineData("/escola", "en-US", "/en/escola")]
    [InlineData("/pt/escola", "ja-JP", "/ja/escola")]
    [InlineData("/blog?pagina=2", "en-US", "/en/blog?pagina=2")]
    [InlineData("/ja/escola#faq", "pt-BR", "/pt/escola#faq")]
    public void LocalizePath_CreatesStableLanguageSpecificUrls(
        string path,
        string culture,
        string expected)
    {
        Assert.Equal(expected, PublicSiteLocales.LocalizePath(path, culture));
    }

    [Theory]
    [InlineData("/pt", "/")]
    [InlineData("/en/escola", "/escola")]
    [InlineData("/ja/blog/post?ref=home", "/blog/post?ref=home")]
    [InlineData("/eventos", "/eventos")]
    public void RemoveLanguagePrefix_PreservesPathAndSuffix(string path, string expected)
    {
        Assert.Equal(expected, PublicSiteLocales.RemoveLanguagePrefix(path));
    }

    [Fact]
    public void OrganizationSchema_UsesOnlyPubliclyVisibleContactFacts()
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("ikkontaiko.example");

        using var document = JsonDocument.Parse(PublicSeoHelper.Organization(context.Request));
        var root = document.RootElement;

        Assert.Equal(
            PublicSeoHelper.OrganizationName,
            root.GetProperty("name").GetString());
        Assert.Equal(
            PublicSeoHelper.Telephone,
            root.GetProperty("telephone").GetString());
        Assert.Equal(
            PublicSeoHelper.Email,
            root.GetProperty("email").GetString());
        Assert.Equal(
            "Rua Domingos de Morais, 2975",
            root.GetProperty("address").GetProperty("streetAddress").GetString());
        Assert.False(root.TryGetProperty("openingHours", out _));
        Assert.False(root.TryGetProperty("review", out _));
        Assert.False(root.TryGetProperty("aggregateRating", out _));
    }

    [Fact]
    public void PublicLayout_ContainsCompleteInternationalAndSocialMetadata()
    {
        var layout = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "Shared",
            "_PublicLayout.cshtml");

        Assert.Contains("rel=\"canonical\"", layout, StringComparison.Ordinal);
        Assert.Contains("hreflang=\"@alternate.Hreflang\"", layout, StringComparison.Ordinal);
        Assert.Contains("hreflang=\"x-default\"", layout, StringComparison.Ordinal);
        Assert.Contains("name=\"robots\"", layout, StringComparison.Ordinal);
        Assert.Contains("property=\"og:locale\"", layout, StringComparison.Ordinal);
        Assert.Contains("name=\"twitter:title\"", layout, StringComparison.Ordinal);
        Assert.Contains("application/ld+json", layout, StringComparison.Ordinal);
    }

    [Fact]
    public void PrivateLayouts_AreExplicitlyNoIndex()
    {
        var layoutNames = new[] { "_Layout.cshtml", "_AlunoLayout.cshtml", "_AuthLayout.cshtml" };

        foreach (var layoutName in layoutNames)
        {
            var layout = ReadRepoFile(
                "IkkonAdmin.Web",
                "Views",
                "Shared",
                layoutName);

            Assert.Contains(
                "noindex,nofollow,noarchive",
                layout,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void SeoDiscoveryEndpoints_AreImplementedAndLocalizedRoutesAreRegistered()
    {
        var seoController = ReadRepoFile(
            "IkkonAdmin.Web",
            "Controllers",
            "SeoController.cs");
        var program = ReadRepoFile("IkkonAdmin.Web", "Program.cs");

        Assert.Contains("[HttpGet(\"/robots.txt\")]", seoController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"/sitemap.xml\")]", seoController, StringComparison.Ordinal);
        Assert.Contains("[HttpGet(\"/llms.txt\")]", seoController, StringComparison.Ordinal);
        Assert.Contains("PublicPathRequestCultureProvider", program, StringComparison.Ordinal);
        Assert.Contains("localized-home", program, StringComparison.Ordinal);
        Assert.Contains("localized-aulas", program, StringComparison.Ordinal);
        Assert.Contains("localized-eventos", program, StringComparison.Ordinal);
        Assert.Contains("localized-galeria", program, StringComparison.Ordinal);

        // A URL histórica precisa continuar roteável para responder o redirect 301.
        Assert.Contains("localized-escola", program, StringComparison.Ordinal);
    }

    [Fact]
    public void LegacySchoolRouteRedirectsPermanentlyToClassesPage()
    {
        var controller = ReadRepoFile(
            "IkkonAdmin.Web",
            "Controllers",
            "InstitucionalController.cs");

        Assert.Contains(
            "RedirectPermanent(i18n.LocalizePath(\"/aulas\"))",
            controller,
            StringComparison.Ordinal);
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var repositoryRoot = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine([repositoryRoot, .. pathSegments]));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "IkkonAdmin.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Não foi possível localizar a raiz do repositório para validar SEO.");
    }
}
