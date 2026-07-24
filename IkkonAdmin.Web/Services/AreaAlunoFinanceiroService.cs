using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoFinanceiroService(
    ApplicationDbContext dbContext,
    IAreaAlunoContextService contextService) : IAreaAlunoFinanceiroService
{
    public async Task<AreaAlunoFinanceiroViewModel?> ObterFinanceiroAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await contextService.ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var mensalidades = await ListarMensalidadesAsync(alunoId.Value, 36, cancellationToken);
        var resumoFinanceiro = await ObterResumoFinanceiroAsync(alunoId.Value, cancellationToken);
        var totalPago = await dbContext.Pagamentos
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId.Value)
            .SumAsync(x => (decimal?)x.ValorPago, cancellationToken) ?? 0m;

        return new AreaAlunoFinanceiroViewModel
        {
            TotalPago = totalPago,
            TotalEmAberto = resumoFinanceiro.TotalEmAberto,
            MensalidadesAtrasadas = resumoFinanceiro.MensalidadesAtrasadas,
            Mensalidades = mensalidades
        };
    }

    public async Task<List<AreaAlunoMensalidadeItemViewModel>> ListarMensalidadesAsync(
        int alunoId,
        int limite,
        CancellationToken cancellationToken = default)
    {
        var mensalidades = await dbContext.Mensalidades
            .AsNoTracking()
            .Include(x => x.Pagamentos)
            .Where(x => x.AlunoId == alunoId)
            .OrderByDescending(x => x.Competencia)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return mensalidades
            .Select(x =>
            {
                var ultimoPagamento = x.Pagamentos
                    .OrderByDescending(p => p.DataPagamento)
                    .FirstOrDefault();

                return new AreaAlunoMensalidadeItemViewModel
                {
                    Id = x.Id,
                    Competencia = x.Competencia,
                    DataVencimento = x.DataVencimento,
                    ValorFinal = x.ValorFinal,
                    Status = x.Status,
                    DataPagamento = x.DataPagamento,
                    FormaPagamento = ultimoPagamento?.FormaPagamento,
                    Comprovante = ultimoPagamento?.Comprovante
                };
            })
            .ToList();
    }

    public async Task<AreaAlunoResumoFinanceiro> ObterResumoFinanceiroAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var totalEmAberto = await dbContext.Mensalidades
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId &&
                        (x.Status == StatusMensalidadeEnum.Pendente || x.Status == StatusMensalidadeEnum.Atrasado))
            .SumAsync(x => (decimal?)x.ValorFinal, cancellationToken) ?? 0m;

        var mensalidadesAtrasadas = await dbContext.Mensalidades
            .AsNoTracking()
            .CountAsync(x => x.AlunoId == alunoId && x.Status == StatusMensalidadeEnum.Atrasado, cancellationToken);

        return new AreaAlunoResumoFinanceiro(totalEmAberto, mensalidadesAtrasadas);
    }
}
