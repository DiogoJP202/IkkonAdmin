using System.Collections.Concurrent;
using IkkonAdmin.Web.Infrastructure.Files;

namespace IkkonAdmin.Tests.Integration;

internal sealed class InMemoryPrivateFileStorage : IPrivateFileStorageService
{
    private readonly ConcurrentDictionary<string, byte[]> files = new(StringComparer.Ordinal);

    public async Task SaveAsync(string storageKey, Stream content, CancellationToken cancellationToken = default)
    {
        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        if (!files.TryAdd(storageKey, buffer.ToArray()))
        {
            throw new IOException($"A chave privada '{storageKey}' já existe.");
        }
    }

    public Task<PrivateFileReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!files.TryGetValue(storageKey, out var content))
        {
            return Task.FromResult<PrivateFileReadResult?>(null);
        }

        Stream stream = new MemoryStream(content, writable: false);
        return Task.FromResult<PrivateFileReadResult?>(new PrivateFileReadResult(stream, content.LongLength));
    }

    public Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        files.TryRemove(storageKey, out _);
        return Task.CompletedTask;
    }
}
