namespace IkkonAdmin.Web.Services;

public interface IPublicSeoService
{
    Task<IReadOnlyList<PublicSitemapPostVersion>> ListPublishedBlogVersionsAsync(
        CancellationToken cancellationToken = default);
}

public sealed record PublicSitemapPostVersion(
    int TranslationGroupId,
    string LanguageCode,
    string Slug,
    DateTime LastModifiedUtc);
