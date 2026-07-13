using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Ganss.Xss;

namespace IkkonAdmin.Web.Services;

public partial class BlogContentSanitizer : IBlogContentSanitizer
{
    private const string EmptyContentHtml = "<p>Conteúdo ainda não informado.</p>";

    public string ConvertPlainTextToSafeHtml(string? content)
    {
        var text = ExtractPlainText(content);
        if (string.IsNullOrWhiteSpace(text))
        {
            return EmptyContentHtml;
        }

        var paragraphs = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(paragraph =>
            {
                var encoded = HtmlEncoder.Default.Encode(paragraph.Trim());
                var withBreaks = encoded.Replace("\n", "<br />", StringComparison.Ordinal);
                return $"<p>{withBreaks}</p>";
            });

        return SanitizeHtml(string.Join(Environment.NewLine, paragraphs));
    }

    public string SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return EmptyContentHtml;
        }

        var htmlWithSafeEmbeds = NormalizeYouTubeIframes(html);
        var sanitizer = CreateSanitizer();
        var sanitized = sanitizer.Sanitize(htmlWithSafeEmbeds);

        return string.IsNullOrWhiteSpace(sanitized) ? EmptyContentHtml : sanitized;
    }

    public string? ExtractPlainText(string? content)
    {
        var trimmed = content?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        return string.Join(
            Environment.NewLine + Environment.NewLine,
            trimmed
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Trim()));
    }

    public string? BuildYouTubeEmbedHtml(string? url)
    {
        var videoId = ExtractYouTubeVideoId(url);
        if (string.IsNullOrWhiteSpace(videoId))
        {
            return null;
        }

        return
            $"""
            <iframe src="https://www.youtube.com/embed/{videoId}" title="Video do YouTube" loading="lazy" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
            """;
    }

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedTags.Clear();
        foreach (var tag in new[]
                 {
                     "p", "br", "strong", "b", "em", "i", "h2", "h3", "ul", "ol", "li",
                     "blockquote", "hr", "a", "img", "figure", "figcaption", "iframe", "span"
                 })
        {
            sanitizer.AllowedTags.Add(tag);
        }

        sanitizer.AllowedAttributes.Clear();
        foreach (var attribute in new[]
                 {
                     "href", "target", "rel", "src", "alt", "title", "width", "height",
                     "loading", "allow", "allowfullscreen", "frameborder", "referrerpolicy"
                 })
        {
            sanitizer.AllowedAttributes.Add(attribute);
        }

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("mailto");

        return sanitizer;
    }

    private static string NormalizeYouTubeIframes(string html)
    {
        var safeEmbeds = new List<string>();
        var withTokens = QuotedIframeRegex().Replace(html, match =>
        {
            var src = match.Groups["src"].Value;
            var safeEmbed = BuildStaticYouTubeEmbedHtml(src);
            if (string.IsNullOrWhiteSpace(safeEmbed))
            {
                return string.Empty;
            }

            var token = $"__IKKON_YOUTUBE_EMBED_{safeEmbeds.Count}__";
            safeEmbeds.Add(safeEmbed);
            return token;
        });

        var withoutUnsafeIframes = AnyIframeRegex().Replace(withTokens, string.Empty);
        for (var index = 0; index < safeEmbeds.Count; index++)
        {
            withoutUnsafeIframes = withoutUnsafeIframes.Replace(
                $"__IKKON_YOUTUBE_EMBED_{index}__",
                safeEmbeds[index],
                StringComparison.Ordinal);
        }

        return withoutUnsafeIframes;
    }

    private static string? BuildStaticYouTubeEmbedHtml(string? url)
    {
        var videoId = ExtractYouTubeVideoId(url);
        if (string.IsNullOrWhiteSpace(videoId))
        {
            return null;
        }

        return
            $"""
            <iframe src="https://www.youtube.com/embed/{videoId}" title="Video do YouTube" loading="lazy" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>
            """;
    }

    private static string? ExtractYouTubeVideoId(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri))
        {
            return null;
        }

        var host = uri.Host.ToLowerInvariant();
        string? videoId = null;

        if (host is "youtu.be")
        {
            videoId = uri.AbsolutePath.Trim('/').Split('/').FirstOrDefault();
        }
        else if (host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase) ||
                 host.EndsWith("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase))
        {
            var pathSegments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments.Length >= 2 && pathSegments[0] is "embed" or "shorts")
            {
                videoId = pathSegments[1];
            }
            else if (uri.AbsolutePath.Equals("/watch", StringComparison.OrdinalIgnoreCase))
            {
                videoId = ParseQuery(uri.Query).GetValueOrDefault("v");
            }
        }

        return IsValidYouTubeVideoId(videoId) ? videoId : null;
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        return query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => part.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(
                parts => Uri.UnescapeDataString(parts[0]),
                parts => Uri.UnescapeDataString(parts[1]),
                StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsValidYouTubeVideoId(string? videoId)
    {
        return !string.IsNullOrWhiteSpace(videoId) &&
               YouTubeVideoIdRegex().IsMatch(videoId);
    }

    [GeneratedRegex("<iframe\\b[^>]*\\bsrc\\s*=\\s*[\"'](?<src>[^\"']+)[\"'][^>]*>\\s*</iframe>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex QuotedIframeRegex();

    [GeneratedRegex("<iframe\\b[^>]*>.*?</iframe>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
    private static partial Regex AnyIframeRegex();

    [GeneratedRegex("^[A-Za-z0-9_-]{11}$", RegexOptions.CultureInvariant)]
    private static partial Regex YouTubeVideoIdRegex();
}
