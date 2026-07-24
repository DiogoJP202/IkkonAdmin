using System.Globalization;

namespace IkkonAdmin.Web.Services;

public sealed class BlogLanguageService : IBlogLanguageService
{
    public string DefaultLanguageCode => "pt-BR";

    public IReadOnlyList<BlogLanguageDefinition> SupportedLanguages { get; } =
    [
        new("pt-BR", "Português", "Português", "pt", true),
        new("en-US", "Inglês", "English", "en", false),
        new("ja-JP", "Japonês", "日本語", "ja", false)
    ];

    public string GetCurrentLanguageCode()
    {
        return Normalize(CultureInfo.CurrentUICulture.Name);
    }

    public string Normalize(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultLanguageCode;
        }

        var normalized = languageCode.Trim();
        var exact = SupportedLanguages.FirstOrDefault(x =>
            string.Equals(x.Code, normalized, StringComparison.OrdinalIgnoreCase));

        if (exact is not null)
        {
            return exact.Code;
        }

        var prefix = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        var byPrefix = SupportedLanguages.FirstOrDefault(x =>
            x.Code.StartsWith($"{prefix}-", StringComparison.OrdinalIgnoreCase));

        return byPrefix?.Code ?? DefaultLanguageCode;
    }

    public BlogLanguageDefinition? GetDefinition(string? languageCode)
    {
        var normalized = Normalize(languageCode);
        return SupportedLanguages.FirstOrDefault(x =>
            string.Equals(x.Code, normalized, StringComparison.OrdinalIgnoreCase));
    }

    public string GetLabel(string? languageCode)
    {
        return GetDefinition(languageCode)?.Label ?? "Português";
    }
}
