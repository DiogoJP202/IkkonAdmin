using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class DashboardService(ApplicationDbContext dbContext) : IDashboardService
{
    private static readonly string[] MesesReferencia =
    [
        "Janeiro",
        "Fevereiro",
        "Marco",
        "Abril",
        "Maio",
        "Junho",
        "Julho",
        "Agosto",
        "Setembro",
        "Outubro",
        "Novembro",
        "Dezembro"
    ];

    public async Task<DashboardViewModel> ObterDashboardAsync(
        int? anoReferencia = null,
        int? mesReferencia = null,
        int? turmaId = null,
        CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var ano = NormalizarAno(anoReferencia ?? DateTime.Today.Year);
        var mes = NormalizarMes(mesReferencia ?? DateTime.Today.Month);
        var competenciaReferencia = new DateOnly(ano, mes, 1);
        var inicioMesFinanceiro = new DateTime(ano, mes, 1);
        var inicioMesFinanceiroSeguinte = inicioMesFinanceiro.AddMonths(1);

        var turmasDisponiveis = await dbContext.Turmas
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new DashboardTurmaFiltroViewModel
            {
                Id = x.Id,
                Nome = x.Nome
            })
            .ToListAsync(cancellationToken);

        if (turmaId.HasValue && turmasDisponiveis.All(x => x.Id != turmaId.Value))
        {
            turmaId = null;
        }

        var alunosAtivosQuery = dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Status == StatusAlunoEnum.Ativo);

        if (turmaId.HasValue)
        {
            alunosAtivosQuery = alunosAtivosQuery.Where(x =>
                x.TurmaId == turmaId.Value ||
                x.AlunoTurmas.Any(t => t.TurmaId == turmaId.Value));
        }

        var mensalidadesBaseQuery = dbContext.Mensalidades
            .AsNoTracking()
            .AsQueryable();

        if (turmaId.HasValue)
        {
            mensalidadesBaseQuery = mensalidadesBaseQuery.Where(x =>
                x.Aluno != null &&
                (x.Aluno.TurmaId == turmaId.Value || x.Aluno.AlunoTurmas.Any(t => t.TurmaId == turmaId.Value)));
        }

        var mensalidadesAtrasadasQuery = mensalidadesBaseQuery.Where(x =>
            x.Competencia <= competenciaReferencia &&
            (x.Status == StatusMensalidadeEnum.Atrasado ||
             (x.Status == StatusMensalidadeEnum.Pendente && x.DataVencimento < hoje)));

        var alunosAtivos = await alunosAtivosQuery.CountAsync(cancellationToken);

        var mensalidadesPendentes = await mensalidadesBaseQuery
            .CountAsync(x =>
                x.Competencia == competenciaReferencia &&
                x.Status == StatusMensalidadeEnum.Pendente, cancellationToken);

        var mensalidadesAtrasadas = await mensalidadesAtrasadasQuery.CountAsync(cancellationToken);

        var totalEmAtraso = await mensalidadesAtrasadasQuery
            .SumAsync(x => (decimal?)x.ValorFinal, cancellationToken) ?? 0m;

        var quantidadeAlunosInadimplentes = await mensalidadesAtrasadasQuery
            .Select(x => x.AlunoId)
            .Distinct()
            .CountAsync(cancellationToken);

        var pagamentosMesQuery = dbContext.Pagamentos
            .AsNoTracking()
            .Where(x => x.DataPagamento >= inicioMesFinanceiro && x.DataPagamento < inicioMesFinanceiroSeguinte);

        if (turmaId.HasValue)
        {
            pagamentosMesQuery = pagamentosMesQuery.Where(x =>
                x.Aluno != null &&
                (x.Aluno.TurmaId == turmaId.Value || x.Aluno.AlunoTurmas.Any(t => t.TurmaId == turmaId.Value)));
        }

        var receitaMes = await pagamentosMesQuery
            .SumAsync(x => (decimal?)x.ValorPago, cancellationToken) ?? 0m;

        var proximosVencimentos = await mensalidadesBaseQuery
            .Where(x =>
                x.Competencia == competenciaReferencia &&
                (x.Status == StatusMensalidadeEnum.Pendente || x.Status == StatusMensalidadeEnum.Atrasado))
            .OrderBy(x => x.DataVencimento)
            .ThenBy(x => x.Aluno!.NomeCompleto)
            .Take(10)
            .Select(x => new ProximoVencimentoViewModel
            {
                MensalidadeId = x.Id,
                AlunoId = x.AlunoId,
                Aluno = x.Aluno!.NomeCompleto,
                Turma = x.Aluno!.Turma != null ? x.Aluno.Turma.Nome : null,
                Vencimento = x.DataVencimento,
                Valor = x.ValorFinal,
                Status = x.Status,
                DiasParaVencimento = x.DataVencimento.DayNumber - hoje.DayNumber
            })
            .ToListAsync(cancellationToken);

        var inadimplenciaPorAluno = await mensalidadesAtrasadasQuery
            .GroupBy(x => x.AlunoId)
            .Select(grupo => new InadimplenciaPorAluno
            {
                AlunoId = grupo.Key,
                QuantidadeMensalidades = grupo.Count(),
                TotalEmAberto = grupo.Sum(x => x.ValorFinal),
                MaiorDiasAtraso = grupo.Max(x => hoje.DayNumber - x.DataVencimento.DayNumber)
            })
            .OrderByDescending(x => x.TotalEmAberto)
            .ThenByDescending(x => x.QuantidadeMensalidades)
            .Take(8)
            .ToListAsync(cancellationToken);

        var idsAlunosInadimplentes = inadimplenciaPorAluno
            .Select(x => x.AlunoId)
            .Distinct()
            .ToArray();

        var alunosInadimplentesLookup = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => idsAlunosInadimplentes.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.NomeCompleto,
                Turma = x.Turma != null ? x.Turma.Nome : null
            })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var inadimplentes = inadimplenciaPorAluno
            .Select(item =>
            {
                alunosInadimplentesLookup.TryGetValue(item.AlunoId, out var alunoInfo);
                return new InadimplenteResumoViewModel
                {
                    AlunoId = item.AlunoId,
                    Aluno = alunoInfo?.NomeCompleto ?? $"Aluno #{item.AlunoId}",
                    Turma = alunoInfo?.Turma,
                    QuantidadeMensalidades = item.QuantidadeMensalidades,
                    TotalEmAberto = item.TotalEmAberto,
                    MaiorDiasAtraso = item.MaiorDiasAtraso
                };
            })
            .ToList();

        var atividadesQuery = dbContext.HistoricosAlunos
            .AsNoTracking()
            .AsQueryable();

        if (turmaId.HasValue)
        {
            atividadesQuery = atividadesQuery.Where(x =>
                x.Aluno != null &&
                (x.Aluno.TurmaId == turmaId.Value || x.Aluno.AlunoTurmas.Any(t => t.TurmaId == turmaId.Value)));
        }

        var atividadesRecentes = await atividadesQuery
            .OrderByDescending(x => x.DataEvento)
            .Take(12)
            .Select(x => new AtividadeRecenteViewModel
            {
                AlunoId = x.AlunoId,
                Data = x.DataEvento,
                TipoEvento = x.TipoEvento,
                Descricao = x.Aluno != null
                    ? $"{x.Aluno.NomeCompleto}: {x.Descricao}"
                    : x.Descricao
            })
            .ToListAsync(cancellationToken);

        return new DashboardViewModel
        {
            QuantidadeAlunosAtivos = alunosAtivos,
            MensalidadesPendentes = mensalidadesPendentes,
            MensalidadesAtrasadas = mensalidadesAtrasadas,
            ReceitaRecebidaNoMes = receitaMes,
            TotalEmAtraso = totalEmAtraso,
            QuantidadeAlunosInadimplentes = quantidadeAlunosInadimplentes,
            AnoReferencia = ano,
            MesReferencia = mes,
            MesAnoReferenciaDescricao = ObterDescricaoMesAno(ano, mes),
            TurmaIdFiltro = turmaId,
            TurmasDisponiveis = turmasDisponiveis,
            ProximosVencimentos = proximosVencimentos,
            Inadimplentes = inadimplentes,
            AtividadesRecentes = atividadesRecentes
        };
    }

    private static int NormalizarAno(int ano)
    {
        return Math.Clamp(ano, 2020, 2100);
    }

    private static int NormalizarMes(int mes)
    {
        return Math.Clamp(mes, 1, 12);
    }

    private static string ObterDescricaoMesAno(int ano, int mes)
    {
        return $"{MesesReferencia[mes - 1]}/{ano}";
    }

    private sealed class InadimplenciaPorAluno
    {
        public int AlunoId { get; set; }
        public int QuantidadeMensalidades { get; set; }
        public decimal TotalEmAberto { get; set; }
        public int MaiorDiasAtraso { get; set; }
    }
}
