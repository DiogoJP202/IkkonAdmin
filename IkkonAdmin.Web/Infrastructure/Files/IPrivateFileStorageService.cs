namespace IkkonAdmin.Web.Infrastructure.Files;

public interface IPrivateFileStorageService
{
    Task SaveAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default);

    Task<PrivateFileReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default);
}

public sealed record PrivateFileReadResult(Stream Content, long Length);
