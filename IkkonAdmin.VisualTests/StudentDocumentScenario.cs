using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

internal sealed class StudentDocumentScenario : IAsyncDisposable
{
    private const string DocumentTypeName = "Documento E2E Playwright";
    private readonly string connectionString;
    private readonly string webContentRoot;
    private readonly int documentTypeId;
    private readonly bool ownsDocumentType;
    private bool disposed;

    private StudentDocumentScenario(
        string connectionString,
        string webContentRoot,
        int requestId,
        int documentTypeId,
        bool ownsDocumentType,
        string marker)
    {
        this.connectionString = connectionString;
        this.webContentRoot = webContentRoot;
        RequestId = requestId;
        this.documentTypeId = documentTypeId;
        this.ownsDocumentType = ownsDocumentType;
        Marker = marker;
    }

    public int RequestId { get; }
    public string Marker { get; }

    public static async Task<StudentDocumentScenario> CreateAsync(
        string connectionString,
        string webContentRoot)
    {
        await using var dbContext = CreateDbContext(connectionString);
        var alunoId = await dbContext.UsuariosSistema
            .AsNoTracking()
            .Where(user => user.LoginNormalizado == "ALUNO.DEMO")
            .Select(user => user.AlunoId)
            .SingleOrDefaultAsync();
        if (!alunoId.HasValue)
        {
            throw new InvalidOperationException(
                "O cenário Playwright exige o usuário de desenvolvimento aluno.demo.");
        }

        var documentType = await dbContext.DocumentoTipos
            .FirstOrDefaultAsync(type => type.Nome == DocumentTypeName);
        var ownsDocumentType = documentType is null;
        if (documentType is null)
        {
            documentType = new DocumentoTipo
            {
                Nome = DocumentTypeName,
                Descricao = "Tipo temporário criado pela regressão de UI.",
                Ativo = true
            };
            dbContext.DocumentoTipos.Add(documentType);
        }

        var marker = $"E2E-{Guid.NewGuid():N}";
        var request = new DocumentoSolicitacao
        {
            AlunoId = alunoId.Value,
            DocumentoTipo = documentType,
            Status = DocumentoStatusEnum.Solicitado,
            DataSolicitacaoUtc = DateTime.UtcNow,
            ObservacaoAdministrativa = marker
        };
        dbContext.DocumentoSolicitacoes.Add(request);
        await dbContext.SaveChangesAsync();

        return new StudentDocumentScenario(
            connectionString,
            webContentRoot,
            request.Id,
            documentType.Id,
            ownsDocumentType,
            marker);
    }

    public async ValueTask DisposeAsync()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;
        await using var dbContext = CreateDbContext(connectionString);
        var request = await dbContext.DocumentoSolicitacoes
            .Include(item => item.Envios)
            .SingleOrDefaultAsync(item => item.Id == RequestId);
        if (request is not null)
        {
            var uploadIds = request.Envios
                .Select(upload => upload.Id.ToString())
                .ToArray();
            var storage = new LocalPrivateFileStorageService(
                new VisualWebHostEnvironment(webContentRoot));
            foreach (var upload in request.Envios)
            {
                await storage.DeleteIfExistsAsync(upload.ArquivoUrl);
            }

            dbContext.DocumentoEnvios.RemoveRange(request.Envios);
            dbContext.DocumentoSolicitacoes.Remove(request);
            var requestId = RequestId.ToString();
            var auditEntries = await dbContext.AuditoriaLogs
                .Where(entry =>
                    (entry.Entidade == nameof(DocumentoSolicitacao) &&
                     entry.EntidadeId == requestId) ||
                    (entry.Entidade == nameof(DocumentoEnvio) &&
                     uploadIds.Contains(entry.EntidadeId!)))
                .ToListAsync();
            dbContext.AuditoriaLogs.RemoveRange(auditEntries);
            await dbContext.SaveChangesAsync();
        }

        if (ownsDocumentType &&
            !await dbContext.DocumentoSolicitacoes.AnyAsync(
                item => item.DocumentoTipoId == documentTypeId))
        {
            var documentType = await dbContext.DocumentoTipos.FindAsync(documentTypeId);
            if (documentType is not null)
            {
                dbContext.DocumentoTipos.Remove(documentType);
                await dbContext.SaveChangesAsync();
            }
        }
    }

    private static ApplicationDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    private sealed class VisualWebHostEnvironment(string contentRootPath) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "IkkonAdmin.VisualTests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.Combine(contentRootPath, "wwwroot");
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ContentRootPath { get; set; } = contentRootPath;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
