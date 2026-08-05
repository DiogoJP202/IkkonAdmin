using Microsoft.AspNetCore.Hosting;

namespace IkkonAdmin.Web.Infrastructure.Files;

public sealed class LocalPrivateFileStorageService(IWebHostEnvironment environment) : IPrivateFileStorageService
{
    private string RootPath => Path.Combine(environment.ContentRootPath, "App_Data", "uploads", "documentos");

    public async Task SaveAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var physicalPath = ResolvePhysicalPath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);

        await using var destination = new FileStream(
            physicalPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        await content.CopyToAsync(destination, cancellationToken);
    }

    public Task<PrivateFileReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var physicalPath = ResolvePhysicalPath(storageKey);
        if (!File.Exists(physicalPath))
        {
            return Task.FromResult<PrivateFileReadResult?>(null);
        }

        var stream = new FileStream(
            physicalPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult<PrivateFileReadResult?>(new PrivateFileReadResult(stream, stream.Length));
    }

    public Task DeleteIfExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var physicalPath = ResolvePhysicalPath(storageKey);
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }

        return Task.CompletedTask;
    }

    private string ResolvePhysicalPath(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.IsPathRooted(storageKey))
        {
            throw new ArgumentException("Chave de storage privado inválida.", nameof(storageKey));
        }

        var normalizedKey = storageKey.Replace('\\', '/').Trim('/');
        var segments = normalizedKey.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("Chave de storage privado inválida.", nameof(storageKey));
        }

        var root = Path.GetFullPath(RootPath);
        var candidate = Path.GetFullPath(Path.Combine(new[] { root }.Concat(segments).ToArray()));
        var expectedPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

        if (!candidate.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A chave de storage privado excede o diretório permitido.", nameof(storageKey));
        }

        return candidate;
    }
}
