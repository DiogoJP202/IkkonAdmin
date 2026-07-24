using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogTextService
{
    string GenerateSlug(string? slug, string fallback);
    string? CleanOptional(string? value);
    string GenerateSafeContentHtml(BlogPostFormViewModel model);
    string? GetContentText(BlogPostFormViewModel model);
    string? SanitizeHtmlForValidation(string? html);
    bool HasHtmlMedia(string? html);
    int CalculateReadingTime(string? contentText);
    List<string> ParseTags(string? tagsInput);
}
