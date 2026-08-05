using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Auditing;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoDocumentosService(
    ApplicationDbContext dbContext,
    IClock clock,
    IPrivateFileStorageService privateFileStorageService,
    IDocumentFileValidator documentFileValidator,
    IAreaAlunoContextService contextService,
    IAuditLogger auditLogger,
    ICurrentUserService currentUserService) : IAreaAlunoDocumentosService
{
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

        var solicitacao = await dbContext.DocumentoSolicitacoes
            .FirstOrDefaultAsync(x => x.Id == solicitacaoId && x.AlunoId == alunoId.Value, cancellationToken);

        if (solicitacao is null)
        {
            await LogDeniedDocumentAccessAsync(usuarioId, solicitacaoId, nameof(DocumentoSolicitacao), cancellationToken);
            return OperationResult.NotFound("Solicitação de documento não encontrada.");
        }

        if (solicitacao.Status == DocumentoStatusEnum.Aprovado)
        {
            return OperationResult.Fail("Este documento já foi aprovado.");
        }

        var validation = await documentFileValidator.ValidateAsync(arquivo, cancellationToken);
        if (!validation.Success || validation.Value is null)
        {
            return OperationResult.Fail(validation.Message, validation.Errors);
        }

        var storageKey = $"{alunoId.Value}/{Guid.NewGuid():N}{validation.Value.Extension}";
        await using (var stream = arquivo.OpenReadStream())
        {
            await privateFileStorageService.SaveAsync(storageKey, stream, cancellationToken);
        }

        try
        {
            dbContext.DocumentoEnvios.Add(new DocumentoEnvio
            {
                DocumentoSolicitacaoId = solicitacao.Id,
                ArquivoUrl = storageKey,
                NomeArquivoOriginal = validation.Value.SafeOriginalFileName,
                ContentType = validation.Value.ContentType,
                TamanhoBytes = arquivo.Length,
                EnviadoEmUtc = clock.UtcNow,
                EnviadoPorUsuarioId = usuarioId
            });

            solicitacao.Status = DocumentoStatusEnum.Enviado;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await privateFileStorageService.DeleteIfExistsAsync(storageKey, CancellationToken.None);
            throw;
        }

        await auditLogger.LogAsync(new AuditLogEntry
        {
            UsuarioResponsavelId = usuarioId,
            UsuarioAfetadoId = usuarioId,
            Acao = AuditEventCodes.DocumentUploaded,
            Entidade = nameof(DocumentoEnvio),
            EntidadeId = solicitacao.Id.ToString(),
            Descricao = "Documento enviado pelo aluno para análise.",
            DadosDepoisJson = AuditJson.Serialize(new
            {
                SolicitacaoId = solicitacao.Id,
                validation.Value.ContentType,
                TamanhoBytes = arquivo.Length
            }),
            EnderecoIp = currentUserService.RemoteIpAddress,
            CorrelationId = currentUserService.CorrelationId
        }, cancellationToken);

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
            await LogDeniedDocumentAccessAsync(usuarioId, envioId, nameof(DocumentoEnvio), cancellationToken);
            return null;
        }

        var storedFile = await privateFileStorageService.OpenReadAsync(envio.ArquivoUrl, cancellationToken);
        if (storedFile is null)
        {
            return null;
        }

        try
        {
            await auditLogger.LogAsync(new AuditLogEntry
            {
                UsuarioResponsavelId = usuarioId,
                UsuarioAfetadoId = usuarioId,
                Acao = AuditEventCodes.DocumentDownloaded,
                Entidade = nameof(DocumentoEnvio),
                EntidadeId = envio.Id.ToString(),
                Descricao = "Documento privado baixado pelo aluno.",
                EnderecoIp = currentUserService.RemoteIpAddress,
                CorrelationId = currentUserService.CorrelationId
            }, cancellationToken);

            return new AreaAlunoDocumentoDownload(
                storedFile.Content,
                DocumentFileValidator.SanitizeDownloadFileName(envio.NomeArquivoOriginal),
                string.IsNullOrWhiteSpace(envio.ContentType) ? "application/octet-stream" : envio.ContentType,
                storedFile.Length);
        }
        catch
        {
            await storedFile.Content.DisposeAsync();
            throw;
        }
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

    private Task LogDeniedDocumentAccessAsync(
        int usuarioId,
        int resourceId,
        string entity,
        CancellationToken cancellationToken)
    {
        return auditLogger.LogAsync(new AuditLogEntry
        {
            UsuarioResponsavelId = usuarioId,
            UsuarioAfetadoId = usuarioId,
            Acao = AuditEventCodes.SensitiveAccessDenied,
            Entidade = entity,
            EntidadeId = resourceId.ToString(),
            Descricao = "Tentativa negada de acesso a documento privado.",
            EnderecoIp = currentUserService.RemoteIpAddress,
            CorrelationId = currentUserService.CorrelationId
        }, cancellationToken);
    }

}
