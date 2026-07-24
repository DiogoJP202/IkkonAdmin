using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public class BlogMediaService(IFileStorageService fileStorageService) : IBlogMediaService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxCoverImageSizeBytes = 3 * 1024 * 1024;
    private const long MaxContentImageSizeBytes = 2 * 1024 * 1024;

    public async Task<BlogMediaSaveResult> SaveCoverImageAsync(
        IFormFile coverImage,
        string? currentCoverUrl,
        CancellationToken cancellationToken = default)
    {
        var saveResult = await SaveImageAsync(
            coverImage,
            ["uploads", "blog", "capas"],
            "/uploads/blog/capas",
            MaxCoverImageSizeBytes,
            "imagem de capa",
            cancellationToken);

        if (!saveResult.Success)
        {
            return saveResult;
        }

        await RemoveCoverImageAsync(currentCoverUrl, cancellationToken);
        return saveResult;
    }

    public Task<BlogMediaSaveResult> SaveContentImageAsync(
        IFormFile contentImage,
        CancellationToken cancellationToken = default)
    {
        return SaveImageAsync(
            contentImage,
            ["uploads", "blog", "conteudo"],
            "/uploads/blog/conteudo",
            MaxContentImageSizeBytes,
            "imagem do conteúdo",
            cancellationToken);
    }

    public Task RemoveCoverImageAsync(string? currentCoverUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentCoverUrl) ||
            !currentCoverUrl.StartsWith("/uploads/blog/capas/", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var filePath = fileStorageService.GetPublicFilePath(
            currentCoverUrl,
            "/uploads/blog/capas/",
            "uploads",
            "blog",
            "capas");

        if (filePath is not null)
        {
            fileStorageService.DeleteIfExists(filePath);
        }

        return Task.CompletedTask;
    }

    private static BlogMediaSaveResult ValidateImage(IFormFile image, long maxSizeBytes, string description)
    {
        if (image.Length <= 0)
        {
            return BlogMediaSaveResult.Fail($"Selecione uma {description} válida.");
        }

        if (image.Length > maxSizeBytes)
        {
            var maxSizeMb = maxSizeBytes / 1024 / 1024;
            return BlogMediaSaveResult.Fail($"A {description} deve ter no máximo {maxSizeMb} MB.");
        }

        var extension = Path.GetExtension(image.FileName ?? string.Empty);
        if (!AllowedImageExtensions.Contains(extension))
        {
            return BlogMediaSaveResult.Fail("Formato de imagem inválido. Use JPG, PNG ou WEBP.");
        }

        return BlogMediaSaveResult.Ok(string.Empty);
    }

    private async Task<BlogMediaSaveResult> SaveImageAsync(
        IFormFile image,
        string[] relativeFolderSegments,
        string publicBaseUrl,
        long maxSizeBytes,
        string description,
        CancellationToken cancellationToken)
    {
        var validation = ValidateImage(image, maxSizeBytes, description);
        if (!validation.Success)
        {
            return validation;
        }

        var extension = Path.GetExtension(image.FileName ?? string.Empty).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var result = await fileStorageService.SaveToWebRootAsync(
            image,
            relativeFolderSegments,
            publicBaseUrl,
            fileName,
            cancellationToken);

        return BlogMediaSaveResult.Ok(result.PublicUrl ?? $"{publicBaseUrl}/{fileName}");
    }
}
