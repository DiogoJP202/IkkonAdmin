namespace IkkonAdmin.Web.Infrastructure.Files;

public sealed record FileStorageResult(
    string FileName,
    string PhysicalPath,
    string? PublicUrl);
