using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class FinanceiroQueryService(
    ApplicationDbContext dbContext,
    IClock clock) : IFinanceiroQueryService
{
    public async Task<FinanceiroIndexViewModel> ObterResumoAsync(
        string? buscaAluno = null,
        StatusMensalidadeEnum? statusFiltro = null,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 5, 100);

        var hoje = DateOnly.FromDateTime(clock.Today);
        var inicioMesAtual = new DateTime(clock.Today.Year, clock.Today.Month, 1);
        var inicioMesSeguinte = inicioMesAtual.AddMonths(1);

        var pendentes = await dbContext.Mensalidades
            .CountAsync(x => x.Status == StatusMensalidadeEnum.Pendente, cancellationToken);

        var atrasadas = await dbContext.Mensalidades
            .CountAsync(x => x.Status == StatusMensalidadeEnum.Atrasado, cancellationToken);

        var valorRecebidoMes = await dbContext.Pagamentos
            .Where(x => x.DataPagamento >= inicioMesAtual && x.DataPagamento < inicioMesSeguinte)
            .SumAsync(x => (decimal?)x.ValorPago, cancellationToken) ?? 0m;

        var valorEmAberto = await dbContext.Mensalidades
            .Where(x => x.Status == StatusMensalidadeEnum.Pendente || x.Status == StatusMensalidadeEnum.Atrasado)
            .SumAsync(x => (decimal?)x.ValorFinal, cancellationToken) ?? 0m;

        var mensalidadesQuery = dbContext.Mensalidades
            .AsNoTracking()
            .Include(x => x.Aluno)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscaAluno))
        {
            var buscaTexto = buscaAluno.Trim();
            mensalidadesQuery = mensalidadesQuery.Where(x =>
                x.Aluno != null &&
                (x.Aluno.NomeCompleto.Contains(buscaTexto) ||
                 x.Aluno.CPF.Contains(buscaTexto) ||
                 (x.Aluno.Celular != null && x.Aluno.Celular.Contains(buscaTexto))));
        }

        if (statusFiltro.HasValue)
        {
            mensalidadesQuery = mensalidadesQuery.Where(x => x.Status == statusFiltro.Value);
        }

        var totalRegistros = await mensalidadesQuery.CountAsync(cancellationToken);

        var mensalidades = await mensalidadesQuery
            .OrderByDescending(x => x.Competencia)
            .ThenBy(x => x.DataVencimento)
            .ThenBy(x => x.Aluno!.NomeCompleto)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new FinanceiroMensalidadeItemViewModel
            {
                MensalidadeId = x.Id,
                AlunoId = x.AlunoId,
                Aluno = x.Aluno!.NomeCompleto,
                Competencia = x.Competencia,
                DataVencimento = x.DataVencimento,
                DataPagamento = x.DataPagamento,
                ValorBase = x.ValorBase,
                ValorFinal = x.ValorFinal,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);

        return new FinanceiroIndexViewModel
        {
            Pendentes = pendentes,
            Atrasadas = atrasadas,
            ValorRecebidoMes = valorRecebidoMes,
            ValorEmAberto = valorEmAberto,
            BuscaAluno = buscaAluno,
            StatusFiltro = statusFiltro,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            MesCompetenciaGeracao = hoje.Month,
            AnoCompetenciaGeracao = hoje.Year,
            Mensalidades = mensalidades
        };
    }

    public async Task<FinanceiroAtrasadosViewModel> ObterAtrasadosAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(clock.Today);

        var itens = await dbContext.Mensalidades
            .AsNoTracking()
            .Include(x => x.Aluno)
            .Where(x => x.Status == StatusMensalidadeEnum.Atrasado)
            .OrderBy(x => x.DataVencimento)
            .ThenBy(x => x.Aluno!.NomeCompleto)
            .Select(x => new FinanceiroAtrasadoItemViewModel
            {
                MensalidadeId = x.Id,
                AlunoId = x.AlunoId,
                Aluno = x.Aluno!.NomeCompleto,
                Competencia = x.Competencia,
                DataVencimento = x.DataVencimento,
                DiasAtraso = hoje.DayNumber - x.DataVencimento.DayNumber,
                ValorFinal = x.ValorFinal
            })
            .ToListAsync(cancellationToken);

        return new FinanceiroAtrasadosViewModel
        {
            TotalEmAtraso = itens.Sum(x => x.ValorFinal),
            Itens = itens
        };
    }

    public async Task<RegistrarPagamentoViewModel?> ObterFormularioPagamentoAsync(
        int mensalidadeId,
        CancellationToken cancellationToken = default)
    {
        var mensalidade = await dbContext.Mensalidades
            .AsNoTracking()
            .Include(x => x.Aluno)
            .FirstOrDefaultAsync(x => x.Id == mensalidadeId, cancellationToken);

        if (mensalidade is null)
        {
            return null;
        }

        return new RegistrarPagamentoViewModel
        {
            MensalidadeId = mensalidade.Id,
            AlunoId = mensalidade.AlunoId,
            AlunoNome = mensalidade.Aluno?.NomeCompleto,
            Competencia = mensalidade.Competencia,
            DataVencimento = mensalidade.DataVencimento,
            ValorMensalidadeAtual = mensalidade.ValorFinal,
            StatusMensalidadeAtual = mensalidade.Status,
            DataPagamento = clock.Now,
            ValorPago = mensalidade.ValorFinal
        };
    }

    public async Task<FinanceiroHistoricoAlunoViewModel?> ObterHistoricoAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var aluno = await dbContext.Alunos
            .AsNoTracking()
            .Include(x => x.Turma)
            .FirstOrDefaultAsync(x => x.Id == alunoId, cancellationToken);

        if (aluno is null)
        {
            return null;
        }

        var mensalidades = await dbContext.Mensalidades
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId)
            .OrderByDescending(x => x.Competencia)
            .ThenByDescending(x => x.DataVencimento)
            .Select(x => new FinanceiroMensalidadeItemViewModel
            {
                MensalidadeId = x.Id,
                AlunoId = x.AlunoId,
                Aluno = aluno.NomeCompleto,
                Competencia = x.Competencia,
                DataVencimento = x.DataVencimento,
                DataPagamento = x.DataPagamento,
                ValorBase = x.ValorBase,
                ValorFinal = x.ValorFinal,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);

        var pagamentos = await dbContext.Pagamentos
            .AsNoTracking()
            .Include(x => x.Mensalidade)
            .Where(x => x.AlunoId == alunoId)
            .OrderByDescending(x => x.DataPagamento)
            .Select(x => new FinanceiroPagamentoItemViewModel
            {
                PagamentoId = x.Id,
                MensalidadeId = x.MensalidadeId,
                Competencia = x.Mensalidade!.Competencia,
                DataPagamento = x.DataPagamento,
                ValorPago = x.ValorPago,
                FormaPagamento = x.FormaPagamento,
                Observacoes = x.Observacoes
            })
            .ToListAsync(cancellationToken);

        return new FinanceiroHistoricoAlunoViewModel
        {
            AlunoId = aluno.Id,
            AlunoNome = aluno.NomeCompleto,
            Turma = aluno.Turma?.Nome,
            TotalPago = pagamentos.Sum(x => x.ValorPago),
            TotalEmAberto = mensalidades
                .Where(x => x.Status == StatusMensalidadeEnum.Pendente || x.Status == StatusMensalidadeEnum.Atrasado)
                .Sum(x => x.ValorFinal),
            Mensalidades = mensalidades,
            Pagamentos = pagamentos
        };
    }
}
