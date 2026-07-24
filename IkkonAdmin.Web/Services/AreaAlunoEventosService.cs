using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoEventosService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAreaAlunoContextService contextService) : IAreaAlunoEventosService
{
    public async Task<AreaAlunoEventosViewModel?> ObterEventosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await contextService.ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        return new AreaAlunoEventosViewModel
        {
            Eventos = await ListarEventosAsync(contexto.AlunoId, contexto.TurmaIds, 100, cancellationToken)
        };
    }

    public async Task<List<AreaAlunoEventoItemViewModel>> ListarEventosAsync(
        int alunoId,
        IReadOnlyCollection<int> turmaIds,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var agora = clock.Now;
        return await dbContext.EventosAlunoPortal
            .AsNoTracking()
            .Where(x => x.Ativo &&
                        x.Fim >= agora &&
                        x.Alvos.Any(a =>
                            a.Todos ||
                            a.AlunoId == alunoId ||
                            (a.TurmaId.HasValue && turmaIds.Contains(a.TurmaId.Value))))
            .OrderByDescending(x => x.Importante)
            .ThenBy(x => x.Inicio)
            .Take(limite)
            .Select(x => new AreaAlunoEventoItemViewModel
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Descricao = x.Descricao,
                Inicio = x.Inicio,
                Fim = x.Fim,
                Local = x.Local,
                Tipo = x.Tipo,
                Importante = x.Importante
            })
            .ToListAsync(cancellationToken);
    }
}
