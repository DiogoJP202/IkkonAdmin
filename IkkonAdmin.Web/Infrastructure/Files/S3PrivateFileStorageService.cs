using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace IkkonAdmin.Web.Infrastructure.Files;

public sealed class S3PrivateFileStorageService(
    IAmazonS3 s3Client,
    IOptions<PrivateFileStorageOptions> options) : IPrivateFileStorageService, IPrivateFileStorageHealthProbe
{
    private readonly PrivateFileStorageOptions settings = options.Value;

    public async Task SaveAsync(
        string storageKey,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        await s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = settings.BucketName,
            Key = ResolveObjectKey(storageKey),
            InputStream = content,
            AutoCloseStream = false,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256
        }, cancellationToken);
    }

    public async Task<PrivateFileReadResult?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await s3Client.GetObjectAsync(
                settings.BucketName,
                ResolveObjectKey(storageKey),
                cancellationToken);
            return new PrivateFileReadResult(
                new ResponseOwnedStream(response.ResponseStream, response),
                response.ContentLength);
        }
        catch (AmazonS3Exception exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task DeleteIfExistsAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        await s3Client.DeleteObjectAsync(
            settings.BucketName,
            ResolveObjectKey(storageKey),
            cancellationToken);
    }

    public async Task CheckAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        await s3Client.GetBucketLocationAsync(new GetBucketLocationRequest
        {
            BucketName = settings.BucketName
        }, cancellationToken);
    }

    private string ResolveObjectKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.IsPathRooted(storageKey))
        {
            throw new ArgumentException("Chave de storage privado inválida.", nameof(storageKey));
        }

        var normalized = storageKey.Replace('\\', '/').Trim('/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("Chave de storage privado inválida.", nameof(storageKey));
        }

        var prefix = settings.KeyPrefix.Replace('\\', '/').Trim('/');
        return string.IsNullOrWhiteSpace(prefix) ? normalized : $"{prefix}/{normalized}";
    }

    private sealed class ResponseOwnedStream(Stream inner, IDisposable owner) : Stream
    {
        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => inner.Position = value; }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override Task FlushAsync(CancellationToken cancellationToken) => inner.FlushAsync(cancellationToken);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
            inner.ReadAsync(buffer, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
                owner.Dispose();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await inner.DisposeAsync();
            owner.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
