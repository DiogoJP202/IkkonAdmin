using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class BlogTagService(
    ApplicationDbContext dbContext,
    IBlogTextService blogTextService) : IBlogTagService
{
    public async Task SyncTagsAsync(
        BlogPost post,
        string? tagsInput,
        CancellationToken cancellationToken = default)
    {
        var names = blogTextService.ParseTags(tagsInput);
        var desiredSlugs = names
            .Select(name => new { Name = name, Slug = blogTextService.GenerateSlug(null, name) })
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .DistinctBy(x => x.Slug, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingTags = desiredSlugs.Count == 0
            ? []
            : await dbContext.BlogTags
                .Where(x => desiredSlugs.Select(y => y.Slug).Contains(x.Slug))
                .ToListAsync(cancellationToken);

        post.PostTags.Clear();

        foreach (var item in desiredSlugs)
        {
            var tag = existingTags.FirstOrDefault(x => x.Slug == item.Slug);
            if (tag is null)
            {
                tag = new BlogTag
                {
                    Name = item.Name,
                    Slug = item.Slug,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                dbContext.BlogTags.Add(tag);
                existingTags.Add(tag);
            }

            post.PostTags.Add(new BlogPostTag
            {
                BlogPost = post,
                BlogTag = tag
            });
        }
    }
}
