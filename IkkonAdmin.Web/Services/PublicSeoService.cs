using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class PublicSeoService(ApplicationDbContext dbContext) : IPublicSeoService
{
    public async Task<IReadOnlyList<PublicSitemapPostVersion>> ListPublishedBlogVersionsAsync(
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;

        return await dbContext.BlogPosts
            .AsNoTracking()
            .Where(post =>
                post.DeletedAtUtc == null &&
                post.Status == BlogPostStatusEnum.Published &&
                post.PublishedAtUtc.HasValue &&
                post.PublishedAtUtc <= nowUtc)
            .OrderBy(post => post.TranslationGroupId ?? post.Id)
            .ThenBy(post => post.LanguageCode)
            .Select(post => new PublicSitemapPostVersion(
                post.TranslationGroupId ?? post.Id,
                post.LanguageCode,
                post.Slug,
                post.UpdatedAtUtc ?? post.PublishedAtUtc ?? post.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
