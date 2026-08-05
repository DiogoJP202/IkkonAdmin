using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Auditing;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class FinanceiroService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAuditLogger auditLogger,
    ICurrentUserService currentUserService) : IFinanceiroService
{
    private const decimal ValorBasePadraoMensalidade = 260m;
    private const int DiaPadraoVencimento = 10;

    public async Task<OperationResult> RegistrarPagamentoAsync(
        RegistrarPagamentoViewModel model,
        CancellationToken cancellationToken = default)
    {
        var mensalidade = await dbContext.Mensalidades
            .FirstOrDefaultAsync(x => x.Id == model.MensalidadeId, cancellationToken);

        if (mensalidade is null)
        {
            return OperationResult.NotFound("Mensalidade não encontrada para registrar pagamento.");
        }

        if (mensalidade.AlunoId != model.AlunoId)
        {
            return OperationResult.Fail("A mensalidade informada não pertence ao aluno selecionado.");
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
        await auditLogger.LogAsync(new AuditLogEntry
        {
            UsuarioResponsavelId = currentUserService.UserId,
            Acao = AuditEventCodes.PaymentRecorded,
            Entidade = nameof(Pagamento),
            EntidadeId = pagamento.Id.ToString(),
            Descricao = "Pagamento de mensalidade registrado.",
            DadosDepoisJson = AuditJson.Serialize(new
            {
                pagamento.MensalidadeId,
                pagamento.AlunoId,
                pagamento.DataPagamento,
                pagamento.ValorPago,
                pagamento.FormaPagamento
            }),
            EnderecoIp = currentUserService.RemoteIpAddress,
            CorrelationId = currentUserService.CorrelationId
        }, cancellationToken);

        return OperationResult.Ok("Pagamento registrado com sucesso.");
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

    public async Task<OperationResult> AtualizarValorFinalAsync(
        int mensalidadeId,
        decimal valorFinal,
        CancellationToken cancellationToken = default)
    {
        if (valorFinal < 0)
        {
            return OperationResult.Fail("Valor final não pode ser negativo.", nameof(Mensalidade.ValorFinal));
        }

        var mensalidade = await dbContext.Mensalidades
            .FirstOrDefaultAsync(x => x.Id == mensalidadeId, cancellationToken);

        if (mensalidade is null)
        {
            return OperationResult.NotFound("Mensalidade não encontrada para atualizar valor.");
        }

        var previousValue = mensalidade.ValorFinal;
        mensalidade.ValorFinal = decimal.Round(valorFinal, 2);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(new AuditLogEntry
        {
            UsuarioResponsavelId = currentUserService.UserId,
            Acao = AuditEventCodes.MonthlyFeeValueChanged,
            Entidade = nameof(Mensalidade),
            EntidadeId = mensalidade.Id.ToString(),
            Descricao = "Valor final da mensalidade alterado.",
            DadosAntesJson = AuditJson.Serialize(new { ValorFinal = previousValue }),
            DadosDepoisJson = AuditJson.Serialize(new { mensalidade.ValorFinal }),
            EnderecoIp = currentUserService.RemoteIpAddress,
            CorrelationId = currentUserService.CorrelationId
        }, cancellationToken);

        return OperationResult.Ok("Valor final atualizado.");
    }

    public async Task<OperationResult> AlterarStatusMensalidadeAsync(
        int mensalidadeId,
        StatusMensalidadeEnum status,
        CancellationToken cancellationToken = default)
    {
        var mensalidade = await dbContext.Mensalidades
            .FirstOrDefaultAsync(x => x.Id == mensalidadeId, cancellationToken);

        if (mensalidade is null)
        {
            return OperationResult.NotFound("Mensalidade não encontrada para alterar status.");
        }

        var previousStatus = mensalidade.Status;
        var previousPaymentDate = mensalidade.DataPagamento;
        mensalidade.Status = status;

        if (status == StatusMensalidadeEnum.Pago)
        {
            mensalidade.DataPagamento ??= DateOnly.FromDateTime(clock.Today);
        }
        else if (status == StatusMensalidadeEnum.Pendente || status == StatusMensalidadeEnum.Cancelado)
        {
            mensalidade.DataPagamento = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(new AuditLogEntry
        {
            UsuarioResponsavelId = currentUserService.UserId,
            Acao = AuditEventCodes.MonthlyFeeStatusChanged,
            Entidade = nameof(Mensalidade),
            EntidadeId = mensalidade.Id.ToString(),
            Descricao = "Status da mensalidade alterado.",
            DadosAntesJson = AuditJson.Serialize(new
            {
                Status = previousStatus,
                DataPagamento = previousPaymentDate
            }),
            DadosDepoisJson = AuditJson.Serialize(new
            {
                mensalidade.Status,
                mensalidade.DataPagamento
            }),
            EnderecoIp = currentUserService.RemoteIpAddress,
            CorrelationId = currentUserService.CorrelationId
        }, cancellationToken);

        return OperationResult.Ok("Status da mensalidade atualizado.");
    }

    public async Task<int> AtualizarAtrasosAsync(CancellationToken cancellationToken = default)
    {
        var config = await ObterConfiguracaoFinanceiraAsync(cancellationToken);
        var hoje = DateOnly.FromDateTime(clock.Today);
        var diasTolerancia = Math.Clamp(config?.DiasToleranciaAtraso ?? 0, 0, 15);
        var limite = hoje.AddDays(-diasTolerancia);

        var pendentesVencidas = await dbContext.Mensalidades
            .Where(x => x.Status == StatusMensalidadeEnum.Pendente && x.DataVencimento < limite)
            .ToListAsync(cancellationToken);

        if (pendentesVencidas.Count == 0)
        {
            return 0;
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
        return pendentesVencidas.Count;
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
