namespace IkkonAdmin.Web.Services;

public interface IBlogSlugService
{
    Task<bool> ExistsAsync(string slug, int? ignoreId, CancellationToken cancellationToken = default);
    Task<string> EnsureUniqueAsync(string baseSlug, int? ignoreId, CancellationToken cancellationToken = default);
}
