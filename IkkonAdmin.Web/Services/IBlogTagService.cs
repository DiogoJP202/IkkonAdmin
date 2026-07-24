using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IBlogTagService
{
    Task SyncTagsAsync(BlogPost post, string? tagsInput, CancellationToken cancellationToken = default);
}
