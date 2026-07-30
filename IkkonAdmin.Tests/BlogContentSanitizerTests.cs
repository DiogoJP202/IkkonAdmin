using IkkonAdmin.Web.Services;

namespace IkkonAdmin.Tests;

public class BlogContentSanitizerTests
{
    private readonly BlogContentSanitizer _sanitizer = new();

    [Fact]
    public void SanitizeHtml_RemoveScriptsEventHandlersAndUnsafeUrls()
    {
        const string html =
            """
            <p onclick="alert('xss')">Texto seguro</p>
            <script>alert('xss')</script>
            <a href="javascript:alert('xss')">Link inseguro</a>
            """;

        var sanitized = _sanitizer.SanitizeHtml(html);

        Assert.Contains("<p>Texto seguro</p>", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("script", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SanitizeHtml_PreservesAllowedContentAndNormalizesYouTubeEmbed()
    {
        const string html =
            """
            <h2>Treino</h2>
            <p><strong>Taiko</strong> em grupo.</p>
            <iframe src="https://youtu.be/dQw4w9WgXcQ"></iframe>
            """;

        var sanitized = _sanitizer.SanitizeHtml(html);

        Assert.Contains("<h2>Treino</h2>", sanitized, StringComparison.Ordinal);
        Assert.Contains("<strong>Taiko</strong>", sanitized, StringComparison.Ordinal);
        Assert.Contains(
            "https://www.youtube.com/embed/dQw4w9WgXcQ",
            sanitized,
            StringComparison.Ordinal);
    }

    [Fact]
    public void SanitizeHtml_RemovesIframeFromUntrustedHost()
    {
        const string html =
            """
            <p>Antes</p>
            <iframe src="https://example.com/embed/video"></iframe>
            <p>Depois</p>
            """;

        var sanitized = _sanitizer.SanitizeHtml(html);

        Assert.Contains("<p>Antes</p>", sanitized, StringComparison.Ordinal);
        Assert.Contains("<p>Depois</p>", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("<iframe", sanitized, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConvertPlainTextToSafeHtml_EncodesMarkupAndPreservesParagraphs()
    {
        var sanitized = _sanitizer.ConvertPlainTextToSafeHtml(
            """
            Primeiro <script>alert('xss')</script>

            Segundo
            """);

        Assert.Contains(
            "&lt;script&gt;alert('xss')&lt;/script&gt;",
            sanitized,
            StringComparison.Ordinal);
        Assert.Contains("<p>Primeiro", sanitized, StringComparison.Ordinal);
        Assert.Contains("<p>Segundo</p>", sanitized, StringComparison.Ordinal);
        Assert.DoesNotContain("<script>", sanitized, StringComparison.OrdinalIgnoreCase);
    }
}
