namespace IkkonAdmin.Web.Services;

public sealed record BlogLanguageDefinition(
    string Code,
    string Label,
    string NativeLabel,
    string SlugSuffix,
    bool IsDefault);
