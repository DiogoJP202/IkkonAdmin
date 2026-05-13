using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogMediaService
{
    Task<BlogMediaSaveResult> SaveCoverImageAsync(IFormFile coverImage, string? currentCoverUrl, CancellationToken cancellationToken = default);
    Task RemoveCoverImageAsync(string? currentCoverUrl, CancellationToken cancellationToken = default);
}
