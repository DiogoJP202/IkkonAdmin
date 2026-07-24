using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Infrastructure.Files;

public sealed class LocalFileStorageService(IWebHostEnvironment webHostEnvironment) : IFileStorageService
{
    public string GetAppDataPath(params string[] relativeSegments)
    {
        return Path.Combine(new[] { webHostEnvironment.ContentRootPath, "App_Data" }.Concat(relativeSegments).ToArray());
    }

    public string GetWebRootPath(params string[] relativeSegments)
    {
        return Path.Combine(new[] { webHostEnvironment.WebRootPath }.Concat(relativeSegments).ToArray());
    }

    public string? GetPublicFilePath(string publicUrl, string expectedPublicPrefix, params string[] rootSegments)
    {
        if (string.IsNullOrWhiteSpace(publicUrl) ||
            !publicUrl.StartsWith(expectedPublicPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var fileName = Path.GetFileName(publicUrl);
        return string.IsNullOrWhiteSpace(fileName)
            ? null
            : GetWebRootPath(rootSegments.Append(fileName).ToArray());
    }

    public bool Exists(string physicalPath)
    {
        return File.Exists(physicalPath);
    }

    public void DeleteIfExists(string physicalPath)
    {
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }

    public Task<FileStorageResult> SaveToAppDataAsync(
        IFormFile file,
        string[] relativeFolderSegments,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var folder = GetAppDataPath(relativeFolderSegments);
        return SaveAsync(file, folder, fileName, publicBaseUrl: null, cancellationToken);
    }

    public Task<FileStorageResult> SaveToWebRootAsync(
        IFormFile file,
        string[] relativeFolderSegments,
        string publicBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        var folder = GetWebRootPath(relativeFolderSegments);
        return SaveAsync(file, folder, fileName, publicBaseUrl, cancellationToken);
    }

    private static async Task<FileStorageResult> SaveAsync(
        IFormFile file,
        string folder,
        string fileName,
        string? publicBaseUrl,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(folder);

        var physicalPath = Path.Combine(folder, fileName);
        await using (var stream = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var publicUrl = string.IsNullOrWhiteSpace(publicBaseUrl)
            ? null
            : $"{publicBaseUrl.TrimEnd('/')}/{fileName}";

        return new FileStorageResult(fileName, physicalPath, publicUrl);
    }
}
