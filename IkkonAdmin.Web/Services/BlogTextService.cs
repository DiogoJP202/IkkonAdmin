using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public sealed class BlogTextService(IBlogContentSanitizer blogContentSanitizer) : IBlogTextService
{
    public string GenerateSlug(string? slug, string fallback)
    {
        var source = string.IsNullOrWhiteSpace(slug) ? fallback : slug;
        var normalized = source.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var finalSlug = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(finalSlug) ? "post" : finalSlug;
    }

    public string? CleanOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    public string GenerateSafeContentHtml(BlogPostFormViewModel model)
    {
        return !string.IsNullOrWhiteSpace(model.ContentHtmlInput)
            ? blogContentSanitizer.SanitizeHtml(model.ContentHtmlInput)
            : blogContentSanitizer.ConvertPlainTextToSafeHtml(model.ContentInput);
    }

    public string? GetContentText(BlogPostFormViewModel model)
    {
        var contentText = blogContentSanitizer.ExtractPlainText(model.ContentInput);
        if (!string.IsNullOrWhiteSpace(contentText))
        {
            return contentText;
        }

        return ExtractTextFromHtml(model.ContentHtmlInput);
    }

    public string? SanitizeHtmlForValidation(string? html)
    {
        return string.IsNullOrWhiteSpace(html)
            ? null
            : blogContentSanitizer.SanitizeHtml(html);
    }

    public bool HasHtmlMedia(string? html)
    {
        return !string.IsNullOrWhiteSpace(html) &&
               Regex.IsMatch(html, "<(img|iframe)\\b", RegexOptions.IgnoreCase);
    }

    public int CalculateReadingTime(string? contentText)
    {
        if (string.IsNullOrWhiteSpace(contentText))
        {
            return 0;
        }

        var wordCount = contentText
            .Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Length;

        return Math.Max(1, (int)Math.Ceiling(wordCount / 200m));
    }

    public List<string> ParseTags(string? tagsInput)
    {
        return (tagsInput ?? string.Empty)
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList();
    }

    private static string? ExtractTextFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var semScripts = Regex.Replace(html, "<(script|style)\\b[^>]*>.*?</\\1>", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var comEspacos = Regex.Replace(semScripts, "</?(p|div|br|li|h[1-6]|blockquote|tr|td|th)\\b[^>]*>", " ", RegexOptions.IgnoreCase);
        var semTags = Regex.Replace(comEspacos, "<[^>]+>", " ");
        var decodificado = WebUtility.HtmlDecode(semTags);
        var normalizado = Regex.Replace(decodificado, "\\s+", " ").Trim();

        return string.IsNullOrWhiteSpace(normalizado) ? null : normalizado;
    }
}
