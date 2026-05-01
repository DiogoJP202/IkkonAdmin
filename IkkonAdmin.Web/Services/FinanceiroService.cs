using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class FinanceiroService(ApplicationDbContext dbContext) : IFinanceiroService
{
    private const decimal ValorBasePadraoMensalidade = 260m;
    private const int DiaPadraoVencimento = 10;

    public async Task<FinanceiroIndexViewModel> ObterResumoAsync(
        string? buscaAluno = null,
        StatusMensalidadeEnum? statusFiltro = null,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 5, 100);

        await AtualizarAtrasosAsync(cancellationToken);

        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var inicioMesAtual = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
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
        await AtualizarAtrasosAsync(cancellationToken);

        var hoje = DateOnly.FromDateTime(DateTime.Today);

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

    public async Task<RegistrarPagamentoViewModel?> ObterFormularioPagamentoAsync(int mensalidadeId, CancellationToken cancellationToken = default)
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
            DataPagamento = DateTime.Now,
            ValorPago = mensalidade.ValorFinal
        };
    }

    public async Task<bool> RegistrarPagamentoAsync(RegistrarPagamentoViewModel model, CancellationToken cancellationToken = default)
    {
        var mensalidade = await dbContext.Mensalidades
            .FirstOrDefaultAsync(x => x.Id == model.MensalidadeId, cancellationToken);

        if (mensalidade is null || mensalidade.AlunoId != model.AlunoId)
        {
            return false;
        }

        var pagamento = new Pagamento
        {
            MensalidadeId = mensalidade.Id,
            AlunoId = mensalidade.AlunoId,
            DataPagamento = model.DataPagamento,
            ValorPago = model.ValorPago,
            FormaPagamento = model.FormaPagamento,
            Observacoes = LimparOpcional(model.Observacoes)
        };

        await dbContext.Pagamentos.AddAsync(pagamento, cancellationToken);

        mensalidade.Status = StatusMensalidadeEnum.Pago;
        mensalidade.DataPagamento = DateOnly.FromDateTime(model.DataPagamento.Date);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FinanceiroGeracaoResultadoViewModel> GerarMensalidadesAsync(int ano, int mes, CancellationToken cancellationToken = default)
    {
        var config = await ObterConfiguracaoFinanceiraAsync(cancellationToken);
        var competencia = new DateOnly(ano, mes, 1);

        var alunosAtivos = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Status == StatusAlunoEnum.Ativo)
            .Select(x => new { x.Id })
            .ToListAsync(cancellationToken);

        var alunosIds = alunosAtivos.Select(x => x.Id).ToArray();

        if (alunosIds.Length == 0)
        {
            return new FinanceiroGeracaoResultadoViewModel();
        }

        var mensalidadesExistentes = await dbContext.Mensalidades
            .AsNoTracking()
            .Where(x => x.Competencia == competencia && alunosIds.Contains(x.AlunoId))
            .Select(x => x.AlunoId)
            .ToListAsync(cancellationToken);

        var alunosComMensalidade = mensalidadesExistentes.ToHashSet();

        var descontos = await dbContext.Descontos
            .AsNoTracking()
            .Where(x => x.Ativo && alunosIds.Contains(x.AlunoId))
            .ToListAsync(cancellationToken);

        var acordos = await dbContext.AcordosFinanceiros
            .AsNoTracking()
            .Where(x => x.Ativo && alunosIds.Contains(x.AlunoId))
            .ToListAsync(cancellationToken);

        var criadas = 0;
        var jaExistentes = 0;
        var valorBasePadrao = config?.ValorMensalidadePadrao ?? ValorBasePadraoMensalidade;
        var diaVencimento = Math.Clamp(config?.DiaVencimentoPadrao ?? DiaPadraoVencimento, 1, 28);
        var dataVencimento = new DateOnly(ano, mes, diaVencimento);

        foreach (var aluno in alunosAtivos)
        {
            if (alunosComMensalidade.Contains(aluno.Id))
            {
                jaExistentes++;
                continue;
            }

            var valorBase = valorBasePadrao;
            var valorFinal = valorBase;

            var acordoAtivo = acordos
                .Where(x => x.AlunoId == aluno.Id && EstaEmVigencia(competencia, x.InicioVigencia, x.FimVigencia))
                .OrderByDescending(x => x.InicioVigencia)
                .FirstOrDefault();

            if (acordoAtivo is not null)
            {
                valorFinal = acordoAtivo.ValorMensalAcordado;
            }

            var descontosAtivos = descontos
                .Where(x => x.AlunoId == aluno.Id && EstaEmVigencia(competencia, x.VigenciaInicio, x.VigenciaFim))
                .ToList();

            foreach (var desconto in descontosAtivos)
            {
                if (desconto.Percentual.HasValue && desconto.Percentual.Value > 0)
                {
                    valorFinal -= valorFinal * (desconto.Percentual.Value / 100m);
                }

                if (desconto.ValorFixo.HasValue && desconto.ValorFixo.Value > 0)
                {
                    valorFinal -= desconto.ValorFixo.Value;
                }
            }

            if (valorFinal < 0)
            {
                valorFinal = 0;
            }

            var mensalidade = new Mensalidade
            {
                AlunoId = aluno.Id,
                Competencia = competencia,
                DataVencimento = dataVencimento,
                ValorBase = valorBase,
                ValorFinal = decimal.Round(valorFinal, 2),
                Status = StatusMensalidadeEnum.Pendente
            };

            await dbContext.Mensalidades.AddAsync(mensalidade, cancellationToken);
            criadas++;
        }

        if (criadas > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new FinanceiroGeracaoResultadoViewModel
        {
            Criadas = criadas,
            JaExistentes = jaExistentes
        };
    }

    public async Task<bool> AtualizarValorFinalAsync(int mensalidadeId, decimal valorFinal, CancellationToken cancellationToken = default)
    {
        var mensalidade = await dbContext.Mensalidades
            .FirstOrDefaultAsync(x => x.Id == mensalidadeId, cancellationToken);

        if (mensalidade is null)
        {
            return false;
        }

        mensalidade.ValorFinal = decimal.Round(valorFinal, 2);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> AlterarStatusMensalidadeAsync(
        int mensalidadeId,
        StatusMensalidadeEnum status,
        CancellationToken cancellationToken = default)
    {
        var mensalidade = await dbContext.Mensalidades
            .FirstOrDefaultAsync(x => x.Id == mensalidadeId, cancellationToken);

        if (mensalidade is null)
        {
            return false;
        }

        mensalidade.Status = status;

        if (status == StatusMensalidadeEnum.Pago)
        {
            mensalidade.DataPagamento ??= DateOnly.FromDateTime(DateTime.Today);
        }
        else if (status == StatusMensalidadeEnum.Pendente || status == StatusMensalidadeEnum.Cancelado)
        {
            mensalidade.DataPagamento = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<FinanceiroHistoricoAlunoViewModel?> ObterHistoricoAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        await AtualizarAtrasosAsync(cancellationToken);

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

    private async Task AtualizarAtrasosAsync(CancellationToken cancellationToken)
    {
        var config = await ObterConfiguracaoFinanceiraAsync(cancellationToken);
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var diasTolerancia = Math.Clamp(config?.DiasToleranciaAtraso ?? 0, 0, 15);
        var limite = hoje.AddDays(-diasTolerancia);

        var pendentesVencidas = await dbContext.Mensalidades
            .Where(x => x.Status == StatusMensalidadeEnum.Pendente && x.DataVencimento < limite)
            .ToListAsync(cancellationToken);

        if (pendentesVencidas.Count == 0)
        {
            return;
        }

        foreach (var mensalidade in pendentesVencidas)
        {
            mensalidade.Status = StatusMensalidadeEnum.Atrasado;

            if (config is not null && config.AplicarMultaJurosAutomaticamente)
            {
                var diasAtraso = Math.Max(1, hoje.DayNumber - mensalidade.DataVencimento.DayNumber);
                var valorOriginal = mensalidade.ValorFinal;

                var multa = valorOriginal * (Math.Max(0m, config.PercentualMultaAtraso) / 100m);
                var jurosDiario = (Math.Max(0m, config.PercentualJurosMes) / 100m) / 30m;
                var juros = valorOriginal * jurosDiario * diasAtraso;

                mensalidade.ValorFinal = decimal.Round(valorOriginal + multa + juros, 2);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<ConfiguracaoSistema?> ObterConfiguracaoFinanceiraAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ConfiguracoesSistema
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static bool EstaEmVigencia(DateOnly competencia, DateOnly inicio, DateOnly? fim)
    {
        return inicio <= competencia && (!fim.HasValue || fim.Value >= competencia);
    }

    private static string? LimparOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
