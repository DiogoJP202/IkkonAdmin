namespace IkkonAdmin.Web.Services;

public interface IBlogLanguageService
{
    string DefaultLanguageCode { get; }
    IReadOnlyList<BlogLanguageDefinition> SupportedLanguages { get; }
    string GetCurrentLanguageCode();
    string Normalize(string? languageCode);
    BlogLanguageDefinition? GetDefinition(string? languageCode);
    string GetLabel(string? languageCode);
}
