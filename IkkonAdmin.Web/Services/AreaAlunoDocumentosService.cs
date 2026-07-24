using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoDocumentosService(
    ApplicationDbContext dbContext,
    IClock clock,
    IFileStorageService fileStorageService,
    IAreaAlunoContextService contextService) : IAreaAlunoDocumentosService
{
    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024;

    public async Task<AreaAlunoDocumentosViewModel?> ObterDocumentosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await contextService.ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        return new AreaAlunoDocumentosViewModel
        {
            Documentos = await ListarDocumentosAsync(alunoId.Value, 100, cancellationToken)
        };
    }

    public async Task<OperationResult> EnviarDocumentoAsync(
        int usuarioId,
        int solicitacaoId,
        IFormFile arquivo,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await contextService.ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return OperationResult.Fail("Conta de aluno não encontrada.");
        }

        if (arquivo.Length <= 0)
        {
            return OperationResult.Fail("Selecione um arquivo para envio.");
        }

        if (arquivo.Length > MaxDocumentSizeBytes)
        {
            return OperationResult.Fail("O arquivo deve ter no máximo 10 MB.");
        }

        var extension = Path.GetExtension(arquivo.FileName ?? string.Empty);
        if (!DocumentExtensions.Contains(extension))
        {
            return OperationResult.Fail("Formato inválido. Use PDF, JPG, PNG ou WEBP.");
        }

        var solicitacao = await dbContext.DocumentoSolicitacoes
            .FirstOrDefaultAsync(x => x.Id == solicitacaoId && x.AlunoId == alunoId.Value, cancellationToken);

        if (solicitacao is null)
        {
            return OperationResult.Fail("Solicitação de documento não encontrada.");
        }

        if (solicitacao.Status == DocumentoStatusEnum.Aprovado)
        {
            return OperationResult.Fail("Este documento já foi aprovado.");
        }

        var fileName = $"{alunoId.Value}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        await fileStorageService.SaveToAppDataAsync(
            arquivo,
            ["uploads", "documentos"],
            fileName,
            cancellationToken);

        dbContext.DocumentoEnvios.Add(new DocumentoEnvio
        {
            DocumentoSolicitacaoId = solicitacao.Id,
            ArquivoUrl = fileName,
            NomeArquivoOriginal = string.IsNullOrWhiteSpace(arquivo.FileName)
                ? fileName
                : Path.GetFileName(arquivo.FileName),
            ContentType = arquivo.ContentType,
            TamanhoBytes = arquivo.Length,
            EnviadoEmUtc = clock.UtcNow,
            EnviadoPorUsuarioId = usuarioId
        });

        solicitacao.Status = DocumentoStatusEnum.Enviado;
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Documento enviado para análise.");
    }

    public async Task<AreaAlunoDocumentoDownload?> ObterDocumentoParaDownloadAsync(
        int usuarioId,
        int envioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await contextService.ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var envio = await dbContext.DocumentoEnvios
            .AsNoTracking()
            .Include(x => x.DocumentoSolicitacao)
            .FirstOrDefaultAsync(
                x => x.Id == envioId &&
                     x.DocumentoSolicitacao != null &&
                     x.DocumentoSolicitacao.AlunoId == alunoId.Value,
                cancellationToken);

        if (envio is null)
        {
            return null;
        }

        var caminho = ObterDocumentoPath(envio.ArquivoUrl);
        return fileStorageService.Exists(caminho)
            ? new AreaAlunoDocumentoDownload(caminho, envio.NomeArquivoOriginal, envio.ContentType)
            : null;
    }

    public async Task<List<AreaAlunoDocumentoItemViewModel>> ListarDocumentosAsync(
        int alunoId,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var solicitacoes = await dbContext.DocumentoSolicitacoes
            .AsNoTracking()
            .Include(x => x.DocumentoTipo)
            .Include(x => x.Envios)
            .Where(x => x.AlunoId == alunoId)
            .OrderBy(x => x.Status == DocumentoStatusEnum.Aprovado)
            .ThenByDescending(x => x.DataSolicitacaoUtc)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return solicitacoes
            .Select(x =>
            {
                var ultimoEnvio = x.Envios
                    .OrderByDescending(e => e.EnviadoEmUtc)
                    .FirstOrDefault();

                return new AreaAlunoDocumentoItemViewModel
                {
                    SolicitacaoId = x.Id,
                    Tipo = x.DocumentoTipo?.Nome ?? $"Documento #{x.DocumentoTipoId}",
                    Descricao = x.DocumentoTipo?.Descricao,
                    Status = x.Status,
                    DataSolicitacaoUtc = x.DataSolicitacaoUtc,
                    DataLimite = x.DataLimite,
                    ObservacaoAdministrativa = x.ObservacaoAdministrativa,
                    UltimoEnvioId = ultimoEnvio?.Id,
                    NomeArquivoOriginal = ultimoEnvio?.NomeArquivoOriginal,
                    EnviadoEmUtc = ultimoEnvio?.EnviadoEmUtc
                };
            })
            .ToList();
    }

    private string ObterDocumentoPath(string fileName)
    {
        return fileStorageService.GetAppDataPath("uploads", "documentos", fileName);
    }
}
