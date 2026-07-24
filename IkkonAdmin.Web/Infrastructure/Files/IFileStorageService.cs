using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Infrastructure.Files;

public interface IFileStorageService
{
    string GetAppDataPath(params string[] relativeSegments);
    string GetWebRootPath(params string[] relativeSegments);
    string? GetPublicFilePath(string publicUrl, string expectedPublicPrefix, params string[] rootSegments);
    bool Exists(string physicalPath);
    void DeleteIfExists(string physicalPath);
    Task<FileStorageResult> SaveToAppDataAsync(
        IFormFile file,
        string[] relativeFolderSegments,
        string fileName,
        CancellationToken cancellationToken = default);
    Task<FileStorageResult> SaveToWebRootAsync(
        IFormFile file,
        string[] relativeFolderSegments,
        string publicBaseUrl,
        string fileName,
        CancellationToken cancellationToken = default);
}
