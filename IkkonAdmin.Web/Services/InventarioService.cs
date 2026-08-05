using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class InventarioService(
    ApplicationDbContext dbContext,
    IClock clock) : IInventarioService
{
    public async Task<OperationResult<int>> CriarAsync(
        InventarioFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var codigo = LimparTextoOpcional(model.CodigoInterno);
        if (!await CodigoDisponivelAsync(codigo, null, cancellationToken))
        {
            return OperationResult<int>.Fail(
                "Código interno já está em uso.",
                nameof(InventarioFormViewModel.CodigoInterno));
        }

        var item = new InventarioItem
        {
            Nome = model.Nome.Trim(),
            CodigoInterno = codigo,
            Categoria = model.Categoria,
            Tipo = LimparTextoOpcional(model.Tipo),
            Descricao = LimparTextoOpcional(model.Descricao),
            Quantidade = model.Quantidade,
            Status = model.Status,
            EstadoConservacao = model.EstadoConservacao,
            Localizacao = LimparTextoOpcional(model.Localizacao),
            DisponivelParaAula = model.DisponivelParaAula,
            DisponivelParaEvento = model.DisponivelParaEvento,
            DataAquisicao = model.DataAquisicao,
            ValorEstimado = model.ValorEstimado,
            Observacoes = LimparTextoOpcional(model.Observacoes),
            CriadoPorUsuarioId = usuarioId,
            CriadoEmUtc = clock.UtcNow,
            Ativo = true
        };

        await dbContext.InventarioItens.AddAsync(item, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(item.Id, "Item cadastrado com sucesso.");
    }

    public async Task<OperationResult<int>> AtualizarAsync(
        int id,
        InventarioFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.InventarioItens
            .FirstOrDefaultAsync(x => x.Id == id && x.Ativo, cancellationToken);

        if (item is null)
        {
            return OperationResult<int>.NotFound("Item não encontrado.");
        }

        var codigo = LimparTextoOpcional(model.CodigoInterno);
        if (!await CodigoDisponivelAsync(codigo, id, cancellationToken))
        {
            return OperationResult<int>.Fail(
                "Código interno já está em uso.",
                nameof(InventarioFormViewModel.CodigoInterno));
        }

        item.Nome = model.Nome.Trim();
        item.CodigoInterno = codigo;
        item.Categoria = model.Categoria;
        item.Tipo = LimparTextoOpcional(model.Tipo);
        item.Descricao = LimparTextoOpcional(model.Descricao);
        item.Quantidade = model.Quantidade;
        item.Status = model.Status;
        item.EstadoConservacao = model.EstadoConservacao;
        item.Localizacao = LimparTextoOpcional(model.Localizacao);
        item.DisponivelParaAula = model.DisponivelParaAula;
        item.DisponivelParaEvento = model.DisponivelParaEvento;
        item.DataAquisicao = model.DataAquisicao;
        item.ValorEstimado = model.ValorEstimado;
        item.Observacoes = LimparTextoOpcional(model.Observacoes);
        item.AtualizadoPorUsuarioId = usuarioId;
        item.AtualizadoEmUtc = clock.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult<int>.Ok(item.Id, "Item atualizado com sucesso.");
    }

    public async Task<OperationResult<int>> InativarAsync(
        int id,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.InventarioItens
            .FirstOrDefaultAsync(x => x.Id == id && x.Ativo, cancellationToken);

        if (item is null)
        {
            return OperationResult<int>.NotFound("Item não encontrado.");
        }

        item.Ativo = false;
        item.Status = InventarioStatusEnum.Baixado;
        item.AtualizadoPorUsuarioId = usuarioId;
        item.AtualizadoEmUtc = clock.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult<int>.Ok(item.Id, "Item baixado do inventário.");
    }

    private async Task<bool> CodigoDisponivelAsync(string? codigoInterno, int? ignorarId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(codigoInterno))
        {
            return true;
        }

        var query = dbContext.InventarioItens.AsQueryable();
        if (ignorarId.HasValue)
        {
            query = query.Where(x => x.Id != ignorarId.Value);
        }

        return !await query.AnyAsync(x => x.CodigoInterno == codigoInterno, cancellationToken);
    }

    private static string? LimparTextoOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
