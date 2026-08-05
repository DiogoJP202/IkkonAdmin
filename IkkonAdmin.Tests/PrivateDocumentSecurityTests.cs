using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Auditing;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class PrivateDocumentSecurityTests
{
    [Fact]
    public async Task AlunoA_NaoEnviaDocumentoParaSolicitacaoDoAlunoB()
    {
        await using var dbContext = CreateDbContext();
        var data = await SeedAsync(dbContext);
        var storage = new InMemoryPrivateFileStorage();
        var audit = new RecordingAuditLogger();
        var service = CreateService(dbContext, storage, audit);

        var result = await service.EnviarDocumentoAsync(
            data.UserA.Id,
            data.RequestB.Id,
            CreatePdf("documento.pdf"));

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(storage.Files);
        Assert.Contains(audit.Entries, x => x.Acao == AuditEventCodes.SensitiveAccessDenied);
    }

    [Fact]
    public async Task AlunoA_NaoBaixaDocumentoDoAlunoB()
    {
        await using var dbContext = CreateDbContext();
        var data = await SeedAsync(dbContext);
        var storage = new InMemoryPrivateFileStorage();
        storage.Files["2/private.pdf"] = "%PDF-1.7"u8.ToArray();
        var audit = new RecordingAuditLogger();
        var service = CreateService(dbContext, storage, audit);

        var download = await service.ObterDocumentoParaDownloadAsync(data.UserA.Id, data.UploadB.Id);

        Assert.Null(download);
        Assert.Equal(0, storage.OpenCount);
        Assert.Contains(audit.Entries, x => x.Acao == AuditEventCodes.SensitiveAccessDenied);
    }

    [Fact]
    public async Task UploadValido_UsaMimeDetectadoEChavePrivada()
    {
        await using var dbContext = CreateDbContext();
        var data = await SeedAsync(dbContext);
        var storage = new InMemoryPrivateFileStorage();
        var audit = new RecordingAuditLogger();
        var service = CreateService(dbContext, storage, audit);

        var result = await service.EnviarDocumentoAsync(
            data.UserA.Id,
            data.RequestA.Id,
            CreatePdf("ficha.pdf", "text/plain"));

        var upload = await dbContext.DocumentoEnvios.SingleAsync(x => x.DocumentoSolicitacaoId == data.RequestA.Id);
        Assert.True(result.Success);
        Assert.Equal("application/pdf", upload.ContentType);
        Assert.StartsWith($"{data.StudentA.Id}/", upload.ArquivoUrl, StringComparison.Ordinal);
        Assert.True(storage.Files.ContainsKey(upload.ArquivoUrl));
        Assert.Contains(audit.Entries, x => x.Acao == AuditEventCodes.DocumentUploaded);
    }

    [Fact]
    public async Task DownloadProprio_RetornaStreamSemExporCaminhoFisicoEAudita()
    {
        await using var dbContext = CreateDbContext();
        var data = await SeedAsync(dbContext);
        var storage = new InMemoryPrivateFileStorage();
        storage.Files["1/private.pdf"] = "%PDF-1.7"u8.ToArray();
        var audit = new RecordingAuditLogger();
        var service = CreateService(dbContext, storage, audit);

        var ownUpload = new DocumentoEnvio
        {
            DocumentoSolicitacaoId = data.RequestA.Id,
            ArquivoUrl = "1/private.pdf",
            NomeArquivoOriginal = "ficha\r\n.pdf",
            ContentType = "application/pdf",
            TamanhoBytes = 8
        };
        dbContext.DocumentoEnvios.Add(ownUpload);
        await dbContext.SaveChangesAsync();

        var download = await service.ObterDocumentoParaDownloadAsync(data.UserA.Id, ownUpload.Id);

        Assert.NotNull(download);
        Assert.IsType<MemoryStream>(download.Content);
        Assert.DoesNotContain('\r', download.NomeArquivoOriginal);
        Assert.DoesNotContain('\n', download.NomeArquivoOriginal);
        Assert.Contains(audit.Entries, x => x.Acao == AuditEventCodes.DocumentDownloaded);
        await download.Content.DisposeAsync();
    }

    private static AreaAlunoDocumentosService CreateService(
        ApplicationDbContext dbContext,
        InMemoryPrivateFileStorage storage,
        RecordingAuditLogger audit)
    {
        return new AreaAlunoDocumentosService(
            dbContext,
            new TestClock(),
            storage,
            new DocumentFileValidator(),
            new AreaAlunoContextService(dbContext),
            audit,
            new StubCurrentUserService());
    }

    private static async Task<TestData> SeedAsync(ApplicationDbContext dbContext)
    {
        var studentA = CreateStudent("Aluno A", "11111111111");
        var studentB = CreateStudent("Aluno B", "22222222222");
        var userA = CreateStudentUser("aluno.a", studentA);
        var userB = CreateStudentUser("aluno.b", studentB);
        var type = new DocumentoTipo { Nome = "Ficha", Ativo = true };
        dbContext.AddRange(studentA, studentB, userA, userB, type);
        await dbContext.SaveChangesAsync();

        var requestA = new DocumentoSolicitacao
        {
            AlunoId = studentA.Id,
            DocumentoTipoId = type.Id,
            Status = DocumentoStatusEnum.Solicitado
        };
        var requestB = new DocumentoSolicitacao
        {
            AlunoId = studentB.Id,
            DocumentoTipoId = type.Id,
            Status = DocumentoStatusEnum.Solicitado
        };
        dbContext.AddRange(requestA, requestB);
        await dbContext.SaveChangesAsync();

        var uploadB = new DocumentoEnvio
        {
            DocumentoSolicitacaoId = requestB.Id,
            ArquivoUrl = "2/private.pdf",
            NomeArquivoOriginal = "privado.pdf",
            ContentType = "application/pdf",
            TamanhoBytes = 8
        };
        dbContext.DocumentoEnvios.Add(uploadB);
        await dbContext.SaveChangesAsync();

        return new TestData(studentA, userA, requestA, requestB, uploadB);
    }

    private static Aluno CreateStudent(string name, string cpf)
    {
        return new Aluno
        {
            NomeCompleto = name,
            CPF = cpf,
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo
        };
    }

    private static UsuarioSistema CreateStudentUser(string login, Aluno student)
    {
        return new UsuarioSistema
        {
            Login = login,
            LoginNormalizado = login.ToUpperInvariant(),
            NomeExibicao = student.NomeCompleto,
            SenhaHash = "hash",
            TipoAcesso = TipoAcessoEnum.Aluno,
            Ativo = true,
            Aluno = student
        };
    }

    private static FormFile CreatePdf(string fileName, string contentType = "application/pdf")
    {
        var bytes = "%PDF-1.7\nconteudo"u8.ToArray();
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "arquivo", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class InMemoryPrivateFileStorage : IPrivateFileStorageService
    {
        public Dictionary<string, byte[]> Files { get; } = [];
        public int OpenCount { get; private set; }

        public async Task SaveAsync(string storageKey, Stream content, CancellationToken cancellationToken = default)
        {
            await using var destination = new MemoryStream();
            await content.CopyToAsync(destination, cancellationToken);
            Files.Add(storageKey, destination.ToArray());
        }

        public Task<PrivateFileReadResult?> OpenReadAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            OpenCount++;
            return Task.FromResult(
                Files.TryGetValue(storageKey, out var content)
                    ? new PrivateFileReadResult(new MemoryStream(content), content.Length)
                    : null);
        }

        public Task DeleteIfExistsAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            Files.Remove(storageKey);
            return Task.CompletedTask;
        }
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow => new(2026, 7, 13, 15, 0, 0, DateTimeKind.Utc);
        public DateTime Now => new(2026, 7, 13, 12, 0, 0);
        public DateTime Today => Now.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }

    private sealed record TestData(
        Aluno StudentA,
        UsuarioSistema UserA,
        DocumentoSolicitacao RequestA,
        DocumentoSolicitacao RequestB,
        DocumentoEnvio UploadB);
}
