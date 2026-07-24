using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoTurmasService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAreaAlunoContextService contextService) : IAreaAlunoTurmasService
{
    public async Task<AreaAlunoTurmasViewModel?> ObterTurmasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await contextService.ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        var turmaPrincipal = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == contexto.AlunoId)
            .Select(x => x.Turma != null ? x.Turma.Nome : null)
            .FirstOrDefaultAsync(cancellationToken);

        return new AreaAlunoTurmasViewModel
        {
            TurmaPrincipal = turmaPrincipal,
            Turmas = await ListarTurmasAsync(contexto.AlunoId, cancellationToken),
            ProximasAulas = await ListarProximasAulasAsync(contexto.TurmaIds, 12, cancellationToken)
        };
    }

    public async Task<AreaAlunoAulasViewModel?> ObterAulasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await contextService.ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        return new AreaAlunoAulasViewModel
        {
            Turmas = await ListarTurmasAsync(contexto.AlunoId, cancellationToken),
            ProximasAulas = await ListarProximasAulasAsync(contexto.TurmaIds, 30, cancellationToken)
        };
    }

    public async Task<List<AreaAlunoTurmaItemViewModel>> ListarTurmasAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var vinculos = await dbContext.AlunosTurmas
            .AsNoTracking()
            .Include(x => x.Turma)
            .ThenInclude(x => x!.Horarios)
            .Include(x => x.Turma)
            .ThenInclude(x => x!.Instrutores)
            .ThenInclude(x => x.UsuarioSistema)
            .Where(x => x.AlunoId == alunoId && x.Turma != null)
            .OrderBy(x => x.Turma!.Nome)
            .ToListAsync(cancellationToken);

        return vinculos
            .Select(x =>
            {
                var instrutor = x.Turma!.Instrutores
                    .Where(i => !i.DataFim.HasValue || i.DataFim.Value >= clock.TodayDate)
                    .OrderByDescending(i => i.Principal)
                    .ThenBy(i => i.DataInicio)
                    .Select(i => i.UsuarioSistema?.NomeExibicao)
                    .FirstOrDefault();

                var horarios = x.Turma.Horarios
                    .Where(h => h.Ativo)
                    .OrderBy(h => h.DiaSemana)
                    .ThenBy(h => h.HoraInicio)
                    .Select(h => new AreaAlunoHorarioItemViewModel
                    {
                        DiaSemana = h.DiaSemana,
                        HoraInicio = h.HoraInicio,
                        HoraFim = h.HoraFim,
                        Local = h.Local
                    })
                    .ToList();

                return new AreaAlunoTurmaItemViewModel
                {
                    Nome = x.Turma.Nome,
                    Modalidade = x.Turma.Modalidade,
                    Horario = x.Turma.Horario,
                    Local = horarios.FirstOrDefault()?.Local,
                    Instrutor = instrutor,
                    Horarios = horarios,
                    DataVinculo = x.DataVinculo
                };
            })
            .ToList();
    }

    public async Task<List<AreaAlunoAulaItemViewModel>> ListarProximasAulasAsync(
        IReadOnlyCollection<int> turmaIds,
        int limite,
        CancellationToken cancellationToken = default)
    {
        if (turmaIds.Count == 0)
        {
            return [];
        }

        var agora = clock.Now;
        return await dbContext.Aulas
            .AsNoTracking()
            .Include(x => x.Turma)
            .Include(x => x.InstrutorUsuario)
            .Where(x => turmaIds.Contains(x.TurmaId) &&
                        x.Status != StatusAulaEnum.Cancelada &&
                        x.Fim >= agora)
            .OrderBy(x => x.Inicio)
            .Take(limite)
            .Select(x => new AreaAlunoAulaItemViewModel
            {
                Id = x.Id,
                Turma = x.Turma != null ? x.Turma.Nome : $"Turma #{x.TurmaId}",
                Modalidade = x.Turma != null ? x.Turma.Modalidade : string.Empty,
                Inicio = x.Inicio,
                Fim = x.Fim,
                Local = x.Local,
                Instrutor = x.InstrutorUsuario != null ? x.InstrutorUsuario.NomeExibicao : null,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }
}
