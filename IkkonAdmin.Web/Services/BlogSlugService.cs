using IkkonAdmin.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class BlogSlugService(ApplicationDbContext dbContext) : IBlogSlugService
{
    public Task<bool> ExistsAsync(
        string slug,
        int? ignoreId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.BlogPosts.AnyAsync(
            x => x.Slug == slug &&
                 (!ignoreId.HasValue || x.Id != ignoreId.Value),
            cancellationToken);
    }

    public async Task<string> EnsureUniqueAsync(
        string baseSlug,
        int? ignoreId,
        CancellationToken cancellationToken = default)
    {
        var slug = baseSlug;
        var suffix = 2;

        while (await ExistsAsync(slug, ignoreId, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }
}
