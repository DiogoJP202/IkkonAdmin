using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoFrequenciaService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAreaAlunoContextService contextService) : IAreaAlunoFrequenciaService
{
    public async Task<AreaAlunoFrequenciaViewModel?> ObterFrequenciaAsync(
        int usuarioId,
        DateOnly? inicio,
        DateOnly? fim,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await contextService.ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var dataFim = fim ?? clock.TodayDate;
        var dataInicio = inicio ?? dataFim.AddMonths(-6);
        if (dataInicio > dataFim)
        {
            (dataInicio, dataFim) = (dataFim, dataInicio);
        }

        var inicioDateTime = dataInicio.ToDateTime(TimeOnly.MinValue);
        var fimDateTime = dataFim.ToDateTime(TimeOnly.MaxValue);

        var registros = await dbContext.FrequenciasAlunos
            .AsNoTracking()
            .Include(x => x.Aula)
            .ThenInclude(x => x!.Turma)
            .Include(x => x.Aula)
            .ThenInclude(x => x!.InstrutorUsuario)
            .Where(x => x.AlunoId == alunoId.Value &&
                        x.Aula != null &&
                        x.Aula.Inicio >= inicioDateTime &&
                        x.Aula.Inicio <= fimDateTime)
            .OrderByDescending(x => x.Aula!.Inicio)
            .ToListAsync(cancellationToken);

        var itens = registros
            .Select(MapearFrequencia)
            .ToList();

        var contabilizados = itens
            .Where(x => x.Status != StatusFrequenciaEnum.Cancelada)
            .ToList();

        var presencas = contabilizados.Count(x => x.Status == StatusFrequenciaEnum.Presente);
        var faltasJustificadas = contabilizados.Count(x => x.Status == StatusFrequenciaEnum.FaltaJustificada || x.Justificada);
        var faltasNaoJustificadas = contabilizados.Count(x => x.Status == StatusFrequenciaEnum.Falta && !x.Justificada);

        return new AreaAlunoFrequenciaViewModel
        {
            Inicio = dataInicio,
            Fim = dataFim,
            TotalRegistros = contabilizados.Count,
            Presencas = presencas,
            FaltasJustificadas = faltasJustificadas,
            FaltasNaoJustificadas = faltasNaoJustificadas,
            PercentualPresenca = CalcularPercentualPresenca(contabilizados.Count, presencas),
            Registros = itens
        };
    }

    public async Task<AreaAlunoResumoFrequencia> ObterResumoFrequenciaAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var inicio = clock.Today.AddMonths(-6);
        var registros = await dbContext.FrequenciasAlunos
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId &&
                        x.Status != StatusFrequenciaEnum.Cancelada &&
                        x.Aula != null &&
                        x.Aula.Inicio >= inicio)
            .Select(x => new { x.Status, x.Justificada })
            .ToListAsync(cancellationToken);

        var presencas = registros.Count(x => x.Status == StatusFrequenciaEnum.Presente);
        var faltasNaoJustificadas = registros.Count(x => x.Status == StatusFrequenciaEnum.Falta && !x.Justificada);

        return new AreaAlunoResumoFrequencia(
            registros.Count,
            presencas,
            faltasNaoJustificadas,
            CalcularPercentualPresenca(registros.Count, presencas));
    }

    public async Task<List<AreaAlunoFrequenciaItemViewModel>> ListarFaltasRecentesAsync(
        int alunoId,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var faltas = await dbContext.FrequenciasAlunos
            .AsNoTracking()
            .Include(x => x.Aula)
            .ThenInclude(x => x!.Turma)
            .Include(x => x.Aula)
            .ThenInclude(x => x!.InstrutorUsuario)
            .Where(x => x.AlunoId == alunoId &&
                        (x.Status == StatusFrequenciaEnum.Falta ||
                         x.Status == StatusFrequenciaEnum.FaltaJustificada))
            .OrderByDescending(x => x.Aula!.Inicio)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return faltas.Select(MapearFrequencia).ToList();
    }

    private static AreaAlunoFrequenciaItemViewModel MapearFrequencia(FrequenciaAluno frequencia)
    {
        return new AreaAlunoFrequenciaItemViewModel
        {
            AulaId = frequencia.AulaId,
            Inicio = frequencia.Aula?.Inicio ?? DateTime.MinValue,
            Turma = frequencia.Aula?.Turma?.Nome ?? "Aula",
            Instrutor = frequencia.Aula?.InstrutorUsuario?.NomeExibicao,
            Status = frequencia.Status,
            Justificada = frequencia.Justificada,
            Justificativa = frequencia.Justificativa
        };
    }

    private static decimal CalcularPercentualPresenca(int total, int presencas)
    {
        return total == 0 ? 0m : decimal.Round((decimal)presencas / total * 100m, 1);
    }
}
