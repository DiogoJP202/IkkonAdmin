using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Helpers;

public sealed record PublicSiteLocale(
    string Segment,
    string Culture,
    string Hreflang,
    string OpenGraphLocale);

public static class PublicSiteLocales
{
    public const string DefaultCulture = "pt-BR";

    public static readonly IReadOnlyList<PublicSiteLocale> All =
    [
        new("pt", DefaultCulture, "pt-BR", "pt_BR"),
        new("en", "en-US", "en", "en_US"),
        new("ja", "ja-JP", "ja", "ja_JP")
    ];

    public static PublicSiteLocale ForCulture(string? culture)
    {
        var normalizedCulture = NormalizeCulture(culture);
        return All.First(locale =>
            locale.Culture.Equals(normalizedCulture, StringComparison.OrdinalIgnoreCase));
    }

    public static bool TryFromSegment(string? segment, out PublicSiteLocale locale)
    {
        locale = All.FirstOrDefault(candidate =>
            candidate.Segment.Equals(segment, StringComparison.OrdinalIgnoreCase))!;

        return locale is not null;
    }

    public static string NormalizeCulture(string? culture)
    {
        if (culture?.StartsWith("ja", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "ja-JP";
        }

        if (culture?.StartsWith("en", StringComparison.OrdinalIgnoreCase) == true)
        {
            return "en-US";
        }

        return DefaultCulture;
    }

    public static string RemoveLanguagePrefix(string? path)
    {
        var (pathOnly, suffix) = SplitPathAndSuffix(path);
        var normalizedPath = NormalizeRootRelativePath(pathOnly);
        var segments = normalizedPath.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments.Length == 0 || !TryFromSegment(segments[0], out _))
        {
            return normalizedPath + suffix;
        }

        var unprefixedPath = segments.Length == 1
            ? "/"
            : $"/{string.Join('/', segments.Skip(1))}";

        return unprefixedPath + suffix;
    }

    public static string LocalizePath(string? path, string? culture)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out _))
        {
            return path!;
        }

        var locale = ForCulture(culture);
        var unprefixedPath = RemoveLanguagePrefix(path);
        var (pathOnly, suffix) = SplitPathAndSuffix(unprefixedPath);
        var normalizedPath = NormalizeRootRelativePath(pathOnly);

        return normalizedPath == "/"
            ? $"/{locale.Segment}{suffix}"
            : $"/{locale.Segment}{normalizedPath}{suffix}";
    }

    public static string AbsoluteUrl(HttpRequest request, string path)
    {
        var normalizedPath = NormalizeRootRelativePath(path);
        return $"{request.Scheme}://{request.Host}{request.PathBase}{normalizedPath}";
    }

    private static (string Path, string Suffix) SplitPathAndSuffix(string? value)
    {
        var path = string.IsNullOrWhiteSpace(value) ? "/" : value.Trim();
        var suffixIndex = path.IndexOfAny(['?', '#']);

        return suffixIndex < 0
            ? (path, string.Empty)
            : (path[..suffixIndex], path[suffixIndex..]);
    }

    private static string NormalizeRootRelativePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var normalizedPath = path.Trim();
        if (!normalizedPath.StartsWith('/'))
        {
            normalizedPath = $"/{normalizedPath.TrimStart('~', '/')}";
        }

        return normalizedPath.Length > 1
            ? normalizedPath.TrimEnd('/')
            : normalizedPath;
    }
}
