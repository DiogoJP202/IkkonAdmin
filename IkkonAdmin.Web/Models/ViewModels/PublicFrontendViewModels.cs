namespace IkkonAdmin.Web.Models.ViewModels;

public sealed record PublicPageActionViewModel(
    string Label,
    string Href,
    string CssClass,
    bool OpensInNewWindow = false);

public sealed record PublicPageIntroViewModel(
    string Kicker,
    string Title,
    string Description,
    PublicPageActionViewModel PrimaryAction,
    PublicPageActionViewModel SecondaryAction);

public sealed record PublicBlogCardPartialViewModel(
    BlogPublicPostCardViewModel Post,
    bool IsFeaturedLayout = false,
    bool ShowBadges = false,
    bool ShowTags = false,
    bool UseCompactReadingLabel = false);

public sealed record PublicAlternateLinkViewModel(
    string Hreflang,
    string Url);

public sealed record PublicBreadcrumbItemViewModel(
    string Label,
    string? Url = null);

public sealed record PublicFaqItemViewModel(
    string Question,
    string Answer);

/// <summary>
/// Unidade física da escola. <paramref name="MapQuery"/> é o endereço completo
/// usado no embed e na rota do Google Maps.
/// </summary>
public sealed record PublicLocationViewModel(
    string Name,
    string Address,
    string MapQuery)
{
    public string EncodedMapQuery => Uri.EscapeDataString(MapQuery);

    public string EmbedUrl => $"https://maps.google.com/maps?q={EncodedMapQuery}&z=16&output=embed";

    public string DirectionsUrl => $"https://www.google.com/maps/dir/?api=1&destination={EncodedMapQuery}";
}
