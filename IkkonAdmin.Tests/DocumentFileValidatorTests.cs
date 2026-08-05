using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Infrastructure.Operations;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Tests;

public class DocumentFileValidatorTests
{
    private readonly DocumentFileValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_ArquivoVazio_Rejeita()
    {
        var file = CreateFile([], "vazio.pdf", "application/pdf");

        var result = await _validator.ValidateAsync(file);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task ValidateAsync_ArquivoGrande_RejeitaAntesDeLerConteudo()
    {
        var stream = new MemoryStream("%PDF-1.7"u8.ToArray());
        var file = new FormFile(stream, 0, DocumentFileValidator.MaxDocumentSizeBytes + 1, "arquivo", "grande.pdf");

        var result = await _validator.ValidateAsync(file);

        Assert.False(result.Success);
        Assert.Contains("10 MB", result.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("arquivo.exe")]
    [InlineData("arquivo.txt")]
    public async Task ValidateAsync_ExtensaoInvalida_Rejeita(string fileName)
    {
        var file = CreateFile("%PDF-1.7"u8.ToArray(), fileName, "application/pdf");

        var result = await _validator.ValidateAsync(file);

        Assert.False(result.Success);
        Assert.Contains("Formato inválido", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ValidateAsync_MimeForjadoComAssinaturaValida_UsaMimeConfiavel()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3 };
        var file = CreateFile(bytes, "imagem.png", "text/plain");

        var result = await _validator.ValidateAsync(file);

        Assert.True(result.Success);
        Assert.NotNull(result.Value);
        Assert.Equal("image/png", result.Value.ContentType);
    }

    [Fact]
    public async Task ValidateAsync_AssinaturaNaoCorrespondeAExtensao_Rejeita()
    {
        var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var file = CreateFile(bytes, "falso.pdf", "application/pdf");

        var result = await _validator.ValidateAsync(file);

        Assert.False(result.Success);
        Assert.Contains("não corresponde", result.Message, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(ValidFiles))]
    public async Task ValidateAsync_AssinaturasPermitidas_Aceita(
        byte[] bytes,
        string fileName,
        string expectedContentType)
    {
        var file = CreateFile(bytes, fileName, "application/octet-stream");

        var result = await _validator.ValidateAsync(file);

        Assert.True(result.Success);
        Assert.Equal(expectedContentType, result.Value?.ContentType);
    }

    public static TheoryData<byte[], string, string> ValidFiles => new()
    {
        { "%PDF-1.7\n"u8.ToArray(), "arquivo.pdf", "application/pdf" },
        { [0xFF, 0xD8, 0xFF, 0xE0], "foto.jpeg", "image/jpeg" },
        { [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], "imagem.png", "image/png" },
        { "RIFF1234WEBP"u8.ToArray(), "imagem.webp", "image/webp" }
    };

    private static FormFile CreateFile(byte[] bytes, string fileName, string contentType)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "arquivo", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
