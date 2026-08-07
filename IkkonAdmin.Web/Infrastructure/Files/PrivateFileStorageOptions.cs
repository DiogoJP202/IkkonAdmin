namespace IkkonAdmin.Web.Infrastructure.Files;

public sealed class PrivateFileStorageOptions
{
    public const string SectionName = "PrivateFileStorage";
    public const string LocalProvider = "Local";
    public const string S3Provider = "S3";

    public string Provider { get; set; } = LocalProvider;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string? ServiceUrl { get; set; }
    public bool ForcePathStyle { get; set; }
    public string KeyPrefix { get; set; } = "documents";
    public string? AccessKeyId { get; set; }
    public string? SecretAccessKey { get; set; }
}

public interface IPrivateFileStorageHealthProbe
{
    Task CheckAvailabilityAsync(CancellationToken cancellationToken = default);
}
