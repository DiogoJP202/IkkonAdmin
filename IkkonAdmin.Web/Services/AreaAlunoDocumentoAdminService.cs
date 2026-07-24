using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoDocumentoAdminService(
    ApplicationDbContext dbContext,
    IClock clock,
    IFileStorageService fileStorageService) : IAreaAlunoDocumentoAdminService
{
    public async Task<AreaAlunoDocumentosAdminViewModel> ObterDocumentosAsync(CancellationToken cancellationToken = default)
    {
        return new AreaAlunoDocumentosAdminViewModel
        {
            Alunos = await ListarAlunosOpcoesAsync(cancellationToken),
            Tipos = await ListarDocumentoTiposAsync(cancellationToken),
            Solicitacoes = await ListarDocumentosRecentesAsync(100, cancellationToken)
        };
    }

    public Task<int> ContarDocumentosPendentesAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.DocumentoSolicitacoes
            .CountAsync(x => x.Status != DocumentoStatusEnum.Aprovado, cancellationToken);
    }

    public async Task<IReadOnlyCollection<AreaAlunoDocumentoAdminItemViewModel>> ListarDocumentosRecentesAsync(
        int limite,
        CancellationToken cancellationToken = default)
    {
        var solicitacoes = await dbContext.DocumentoSolicitacoes
            .AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.DocumentoTipo)
            .Include(x => x.Envios)
            .OrderBy(x => x.Status == DocumentoStatusEnum.Aprovado)
            .ThenByDescending(x => x.DataSolicitacaoUtc)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return solicitacoes
            .Select(x =>
            {
                var ultimoEnvio = x.Envios.OrderByDescending(e => e.EnviadoEmUtc).FirstOrDefault();
                return new AreaAlunoDocumentoAdminItemViewModel
                {
                    SolicitacaoId = x.Id,
                    AlunoId = x.AlunoId,
                    Aluno = x.Aluno?.NomeCompleto ?? $"Aluno #{x.AlunoId}",
                    DocumentoTipoId = x.DocumentoTipoId,
                    Tipo = x.DocumentoTipo?.Nome ?? $"Documento #{x.DocumentoTipoId}",
                    Status = x.Status,
                    DataSolicitacaoUtc = x.DataSolicitacaoUtc,
                    DataLimite = x.DataLimite,
                    ObservacaoAdministrativa = x.ObservacaoAdministrativa,
                    Envios = x.Envios.Count,
                    UltimoEnvioId = ultimoEnvio?.Id,
                    NomeArquivoOriginal = ultimoEnvio?.NomeArquivoOriginal
                };
            })
            .ToList();
    }

    public async Task<OperationResult> CriarDocumentoTipoAsync(
        DocumentoTipoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var nome = model.Nome.Trim();
        var existe = await dbContext.DocumentoTipos.AnyAsync(x => x.Nome == nome, cancellationToken);
        if (existe)
        {
            return OperationResult.Fail("Já existe um tipo de documento com este nome.");
        }

        await dbContext.DocumentoTipos.AddAsync(new DocumentoTipo
        {
            Nome = nome,
            Descricao = LimparOpcional(model.Descricao),
            Obrigatorio = model.Obrigatorio,
            Ativo = model.Ativo
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Tipo de documento criado.");
    }

    public async Task<OperationResult> AtualizarDocumentoTipoAsync(
        int id,
        DocumentoTipoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var tipo = await dbContext.DocumentoTipos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (tipo is null)
        {
            return OperationResult.Fail("Tipo de documento não encontrado.");
        }

        var nome = model.Nome.Trim();
        var existe = await dbContext.DocumentoTipos
            .AnyAsync(x => x.Id != id && x.Nome == nome, cancellationToken);

        if (existe)
        {
            return OperationResult.Fail("Já existe um tipo de documento com este nome.");
        }

        tipo.Nome = nome;
        tipo.Descricao = LimparOpcional(model.Descricao);
        tipo.Obrigatorio = model.Obrigatorio;
        tipo.Ativo = model.Ativo;

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Tipo de documento atualizado.");
    }

    public async Task<OperationResult> ExcluirDocumentoTipoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var tipo = await dbContext.DocumentoTipos
            .Include(x => x.Solicitacoes)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tipo is null)
        {
            return OperationResult.Fail("Tipo de documento não encontrado.");
        }

        if (tipo.Solicitacoes.Count > 0)
        {
            tipo.Ativo = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Ok("Tipo desativado porque possui solicitações vinculadas.");
        }

        dbContext.DocumentoTipos.Remove(tipo);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Tipo de documento excluído.");
    }

    public async Task<OperationResult> SolicitarDocumentoAsync(
        DocumentoSolicitacaoFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var tipoExiste = await dbContext.DocumentoTipos.AnyAsync(x => x.Id == model.DocumentoTipoId && x.Ativo, cancellationToken);
        var alunoExiste = await dbContext.Alunos.AnyAsync(x => x.Id == model.AlunoId, cancellationToken);

        if (!tipoExiste || !alunoExiste)
        {
            return OperationResult.Fail("Tipo de documento ou aluno inválido.");
        }

        await dbContext.DocumentoSolicitacoes.AddAsync(new DocumentoSolicitacao
        {
            DocumentoTipoId = model.DocumentoTipoId,
            AlunoId = model.AlunoId,
            SolicitadoPorUsuarioId = usuarioId,
            Status = DocumentoStatusEnum.Solicitado,
            DataSolicitacaoUtc = clock.UtcNow,
            DataLimite = model.DataLimite,
            ObservacaoAdministrativa = LimparOpcional(model.ObservacaoAdministrativa)
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Documento solicitado.");
    }

    public async Task<OperationResult> AtualizarDocumentoSolicitacaoAsync(
        int id,
        DocumentoSolicitacaoEdicaoViewModel model,
        CancellationToken cancellationToken = default)
    {
        var solicitacao = await dbContext.DocumentoSolicitacoes
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (solicitacao is null)
        {
            return OperationResult.Fail("Solicitação não encontrada.");
        }

        var tipoExiste = await dbContext.DocumentoTipos.AnyAsync(x => x.Id == model.DocumentoTipoId, cancellationToken);
        var alunoExiste = await dbContext.Alunos.AnyAsync(x => x.Id == model.AlunoId, cancellationToken);

        if (!tipoExiste || !alunoExiste)
        {
            return OperationResult.Fail("Tipo de documento ou aluno inválido.");
        }

        solicitacao.DocumentoTipoId = model.DocumentoTipoId;
        solicitacao.AlunoId = model.AlunoId;
        solicitacao.Status = model.Status;
        solicitacao.DataLimite = model.DataLimite;
        solicitacao.ObservacaoAdministrativa = LimparOpcional(model.ObservacaoAdministrativa);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Solicitação atualizada.");
    }

    public async Task<OperationResult> ExcluirDocumentoSolicitacaoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var solicitacao = await dbContext.DocumentoSolicitacoes
            .Include(x => x.Envios)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (solicitacao is null)
        {
            return OperationResult.Fail("Solicitação não encontrada.");
        }

        if (solicitacao.Envios.Count > 0)
        {
            return OperationResult.Fail("Não é possível excluir uma solicitação com arquivos enviados. Altere o status para recusado ou pendente.");
        }

        dbContext.DocumentoSolicitacoes.Remove(solicitacao);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Solicitação excluída.");
    }

    public async Task<OperationResult> AvaliarDocumentoAsync(
        DocumentoAvaliacaoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var solicitacao = await dbContext.DocumentoSolicitacoes
            .FirstOrDefaultAsync(x => x.Id == model.SolicitacaoId, cancellationToken);

        if (solicitacao is null)
        {
            return OperationResult.Fail("Solicitação não encontrada.");
        }

        solicitacao.Status = model.Status;
        solicitacao.ObservacaoAdministrativa = LimparOpcional(model.ObservacaoAdministrativa);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Documento atualizado.");
    }

    public async Task<AreaAlunoDocumentoDownload?> ObterDocumentoAdminDownloadAsync(
        int envioId,
        CancellationToken cancellationToken = default)
    {
        var envio = await dbContext.DocumentoEnvios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == envioId, cancellationToken);

        if (envio is null)
        {
            return null;
        }

        var caminho = ObterDocumentoPath(envio.ArquivoUrl);
        return fileStorageService.Exists(caminho)
            ? new AreaAlunoDocumentoDownload(caminho, envio.NomeArquivoOriginal, envio.ContentType)
            : null;
    }

    private async Task<IReadOnlyCollection<AreaAlunoOpcaoViewModel>> ListarAlunosOpcoesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Status != StatusAlunoEnum.Desligado)
            .OrderBy(x => x.NomeCompleto)
            .Select(x => new AreaAlunoOpcaoViewModel
            {
                Id = x.Id,
                Nome = x.NomeCompleto
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoDocumentoTipoItemViewModel>> ListarDocumentoTiposAsync(CancellationToken cancellationToken)
    {
        return await dbContext.DocumentoTipos
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new AreaAlunoDocumentoTipoItemViewModel
            {
                Id = x.Id,
                Nome = x.Nome,
                Descricao = x.Descricao,
                Obrigatorio = x.Obrigatorio,
                Ativo = x.Ativo
            })
            .ToListAsync(cancellationToken);
    }

    private string ObterDocumentoPath(string fileName)
    {
        return fileStorageService.GetAppDataPath("uploads", "documentos", fileName);
    }

    private static string? LimparOpcional(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}
