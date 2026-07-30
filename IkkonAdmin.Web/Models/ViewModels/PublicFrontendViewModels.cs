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
