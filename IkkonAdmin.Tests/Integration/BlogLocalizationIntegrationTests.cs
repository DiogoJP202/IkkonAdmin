using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using System.Net;

namespace IkkonAdmin.Tests.Integration;

public sealed class BlogLocalizationIntegrationTests
{
    [Fact]
    public async Task Blog_SelecionaIdiomaExato_FazFallbackPtEAgrupaTraducoesPublicadas()
    {
        await using var factory = new IkkonWebApplicationFactory();
        await factory.SeedAsync(async dbContext =>
        {
            var root = CreatePublishedPost("Título raiz PT", "titulo-raiz-pt", "pt-BR");
            var fallback = CreatePublishedPost("Somente em português", "somente-portugues", "pt-BR");
            dbContext.BlogPosts.AddRange(root, fallback);
            await dbContext.SaveChangesAsync();

            dbContext.BlogPosts.AddRange(
                CreatePublishedPost("Exact English version", "exact-english-version", "en-US", root.Id),
                CreatePublishedPost("日本語の正確な記事", "nihongo-seikaku", "ja-JP", root.Id),
                new BlogPost
                {
                    Title = "Future English post",
                    Slug = "future-english-post",
                    LanguageCode = "en-US",
                    Status = BlogPostStatusEnum.Published,
                    PublishedAtUtc = DateTime.UtcNow.AddDays(2),
                    CreatedAtUtc = DateTime.UtcNow
                },
                new BlogPost
                {
                    Title = "Rascunho japonês",
                    Slug = "rascunho-japones",
                    LanguageCode = "ja-JP",
                    Status = BlogPostStatusEnum.Draft,
                    CreatedAtUtc = DateTime.UtcNow
                });
        });

        using var client = factory.CreateClient();
        var english = WebUtility.HtmlDecode(await client.GetStringAsync("/en/blog"));
        Assert.Contains("Exact English version", english);
        Assert.Contains("Somente em português", english);
        Assert.DoesNotContain("Título raiz PT", english);
        Assert.DoesNotContain("Future English post", english);

        var japanese = WebUtility.HtmlDecode(await client.GetStringAsync("/ja/blog"));
        Assert.Contains("日本語の正確な記事", japanese);
        Assert.Contains("Somente em português", japanese);
        Assert.DoesNotContain("Rascunho japonês", japanese);

        var details = WebUtility.HtmlDecode(await client.GetStringAsync("/ja/blog/nihongo-seikaku"));
        Assert.Contains("日本語の正確な記事", details);
        Assert.Contains("/en/blog/exact-english-version", details);
        Assert.Contains("/blog/titulo-raiz-pt", details);
    }

    private static BlogPost CreatePublishedPost(
        string title,
        string slug,
        string language,
        int? translationGroupId = null)
    {
        return new BlogPost
        {
            Title = title,
            Slug = slug,
            Summary = $"Resumo de {title}",
            ContentHtml = $"<p>{title}</p>",
            ContentText = title,
            LanguageCode = language,
            TranslationGroupId = translationGroupId,
            Status = BlogPostStatusEnum.Published,
            PublishedAtUtc = DateTime.UtcNow.AddDays(-1),
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2),
            ReadingTimeMinutes = 2
        };
    }
}
