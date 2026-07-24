using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoComunicadosService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAreaAlunoContextService contextService) : IAreaAlunoComunicadosService
{
    public async Task<AreaAlunoComunicadosViewModel?> ObterComunicadosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await contextService.ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        return new AreaAlunoComunicadosViewModel
        {
            Comunicados = await ListarComunicadosAsync(contexto.AlunoId, contexto.TurmaIds, 100, cancellationToken)
        };
    }

    public async Task<bool> MarcarComunicadoComoLidoAsync(
        int usuarioId,
        int comunicadoId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await contextService.ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return false;
        }

        var podeLer = await ComunicadoPertenceAoAlunoAsync(
            comunicadoId,
            contexto.AlunoId,
            contexto.TurmaIds,
            cancellationToken);

        if (!podeLer)
        {
            return false;
        }

        var jaLido = await dbContext.ComunicadosLeituras
            .AnyAsync(x => x.ComunicadoId == comunicadoId && x.AlunoId == contexto.AlunoId, cancellationToken);

        if (!jaLido)
        {
            dbContext.ComunicadosLeituras.Add(new ComunicadoLeitura
            {
                ComunicadoId = comunicadoId,
                AlunoId = contexto.AlunoId,
                LidoEmUtc = clock.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<List<AreaAlunoComunicadoItemViewModel>> ListarComunicadosAsync(
        int alunoId,
        IReadOnlyCollection<int> turmaIds,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var agora = clock.UtcNow;
        return await dbContext.Comunicados
            .AsNoTracking()
            .Where(x => x.Ativo &&
                        x.PublicadoEmUtc <= agora &&
                        (!x.ExpiraEmUtc.HasValue || x.ExpiraEmUtc.Value >= agora) &&
                        x.Alvos.Any(a =>
                            a.Todos ||
                            a.AlunoId == alunoId ||
                            (a.TurmaId.HasValue && turmaIds.Contains(a.TurmaId.Value))))
            .OrderByDescending(x => x.Fixado)
            .ThenByDescending(x => x.Importante)
            .ThenByDescending(x => x.PublicadoEmUtc)
            .Take(limite)
            .Select(x => new AreaAlunoComunicadoItemViewModel
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Conteudo = x.Conteudo,
                Importante = x.Importante,
                Fixado = x.Fixado,
                PublicadoEmUtc = x.PublicadoEmUtc,
                ExpiraEmUtc = x.ExpiraEmUtc,
                Lido = x.Leituras.Any(l => l.AlunoId == alunoId)
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<bool> ComunicadoPertenceAoAlunoAsync(
        int comunicadoId,
        int alunoId,
        IReadOnlyCollection<int> turmaIds,
        CancellationToken cancellationToken)
    {
        var agora = clock.UtcNow;
        return await dbContext.Comunicados
            .AsNoTracking()
            .AnyAsync(x => x.Id == comunicadoId &&
                           x.Ativo &&
                           x.PublicadoEmUtc <= agora &&
                           (!x.ExpiraEmUtc.HasValue || x.ExpiraEmUtc.Value >= agora) &&
                           x.Alvos.Any(a =>
                               a.Todos ||
                               a.AlunoId == alunoId ||
                               (a.TurmaId.HasValue && turmaIds.Contains(a.TurmaId.Value))),
                cancellationToken);
    }
}
