using IkkonAdmin.Web.Infrastructure.Operations;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Infrastructure.Files;

public sealed class DocumentFileValidator : IDocumentFileValidator
{
    public const long MaxDocumentSizeBytes = 10 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, DocumentFormat> Formats =
        new Dictionary<string, DocumentFormat>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = new(".pdf", "application/pdf", bytes => StartsWith(bytes, "%PDF-"u8)),
            [".jpg"] = new(".jpg", "image/jpeg", IsJpeg),
            [".jpeg"] = new(".jpg", "image/jpeg", IsJpeg),
            [".png"] = new(".png", "image/png", bytes => StartsWith(bytes, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])),
            [".webp"] = new(".webp", "image/webp", IsWebp)
        };

    public async Task<OperationResult<DocumentFileValidationResult>> ValidateAsync(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file.Length <= 0)
        {
            return OperationResult<DocumentFileValidationResult>.Fail("Selecione um arquivo não vazio para envio.");
        }

        if (file.Length > MaxDocumentSizeBytes)
        {
            return OperationResult<DocumentFileValidationResult>.Fail("O arquivo deve ter no máximo 10 MB.");
        }

        var uploadedExtension = Path.GetExtension(file.FileName ?? string.Empty);
        if (!Formats.TryGetValue(uploadedExtension, out var expectedFormat))
        {
            return OperationResult<DocumentFileValidationResult>.Fail("Formato inválido. Use PDF, JPG, PNG ou WEBP.");
        }

        var header = new byte[16];
        await using var stream = file.OpenReadStream();
        var bytesRead = 0;
        while (bytesRead < header.Length)
        {
            var read = await stream.ReadAsync(header.AsMemory(bytesRead, header.Length - bytesRead), cancellationToken);
            if (read == 0)
            {
                break;
            }

            bytesRead += read;
        }

        var signature = header.AsSpan(0, bytesRead);
        if (!expectedFormat.Matches(signature))
        {
            return OperationResult<DocumentFileValidationResult>.Fail(
                "O conteúdo do arquivo não corresponde ao formato informado.");
        }

        var originalName = SanitizeDownloadFileName(file.FileName, expectedFormat.Extension);
        return OperationResult<DocumentFileValidationResult>.Ok(
            new DocumentFileValidationResult(
                expectedFormat.Extension,
                expectedFormat.ContentType,
                originalName),
            "Arquivo válido.");
    }

    public static string SanitizeDownloadFileName(string? fileName, string fallbackExtension = ".bin")
    {
        var safeName = Path.GetFileName(fileName ?? string.Empty)
            .Replace('\r', '_')
            .Replace('\n', '_')
            .Replace('"', '_');

        foreach (var invalidCharacter in Path.GetInvalidFileNameChars())
        {
            safeName = safeName.Replace(invalidCharacter, '_');
        }

        safeName = safeName.Trim();
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = $"documento{fallbackExtension}";
        }

        return safeName.Length <= 180 ? safeName : safeName[..180];
    }

    private static bool IsJpeg(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
    }

    private static bool IsWebp(ReadOnlySpan<byte> bytes)
    {
        return bytes.Length >= 12 &&
               StartsWith(bytes, "RIFF"u8) &&
               bytes.Slice(8, 4).SequenceEqual("WEBP"u8);
    }

    private static bool StartsWith(ReadOnlySpan<byte> bytes, ReadOnlySpan<byte> signature)
    {
        return bytes.Length >= signature.Length && bytes[..signature.Length].SequenceEqual(signature);
    }

    private sealed record DocumentFormat(
        string Extension,
        string ContentType,
        Func<ReadOnlySpan<byte>, bool> Matches);
}
