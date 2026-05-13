using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public class BlogMediaService(IWebHostEnvironment webHostEnvironment) : IBlogMediaService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxCoverImageSizeBytes = 3 * 1024 * 1024;

    public async Task<BlogMediaSaveResult> SaveCoverImageAsync(
        IFormFile coverImage,
        string? currentCoverUrl,
        CancellationToken cancellationToken = default)
    {
        if (coverImage.Length <= 0)
        {
            return BlogMediaSaveResult.Fail("Selecione uma imagem de capa valida.");
        }

        if (coverImage.Length > MaxCoverImageSizeBytes)
        {
            return BlogMediaSaveResult.Fail("A imagem de capa deve ter no maximo 3 MB.");
        }

        var extension = Path.GetExtension(coverImage.FileName ?? string.Empty);
        if (!AllowedImageExtensions.Contains(extension))
        {
            return BlogMediaSaveResult.Fail("Formato de imagem invalido. Use JPG, PNG ou WEBP.");
        }

        var coversFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "blog", "capas");
        Directory.CreateDirectory(coversFolder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(coversFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await coverImage.CopyToAsync(stream, cancellationToken);
        }

        await RemoveCoverImageAsync(currentCoverUrl, cancellationToken);
        return BlogMediaSaveResult.Ok($"/uploads/blog/capas/{fileName}");
    }

    public Task RemoveCoverImageAsync(string? currentCoverUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentCoverUrl) ||
            !currentCoverUrl.StartsWith("/uploads/blog/capas/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var coversFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "blog", "capas");
        var fileName = Path.GetFileName(currentCoverUrl);
        var filePath = Path.Combine(coversFolder, fileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }
}
