namespace IkkonAdmin.Web.Services;

public interface IBlogContentSanitizer
{
    string ConvertPlainTextToSafeHtml(string? content);
    string SanitizeHtml(string? html);
    string? ExtractPlainText(string? content);
    string? BuildYouTubeEmbedHtml(string? url);
}
