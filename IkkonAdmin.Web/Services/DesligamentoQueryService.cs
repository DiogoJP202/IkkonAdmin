using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class DesligamentoQueryService(
    ApplicationDbContext dbContext,
    IClock clock) : IDesligamentoQueryService
{
    public async Task<IReadOnlyList<Desligamento>> ListarAsync(
        string? busca = null,
        bool? confirmado = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Desligamentos
            .AsNoTracking()
            .Include(x => x.Aluno)
            .ThenInclude(x => x!.Turma)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaTexto = busca.Trim();
            query = query.Where(x =>
                x.Aluno != null &&
                (x.Aluno.NomeCompleto.Contains(buscaTexto) ||
                 x.Aluno.CPF.Contains(buscaTexto)));
        }

        if (confirmado.HasValue)
        {
            query = confirmado.Value
                ? query.Where(x => x.DataConfirmacao.HasValue)
                : query.Where(x => !x.DataConfirmacao.HasValue);
        }

        return await query
            .OrderByDescending(x => x.DataSolicitacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Aluno>> ListarAlunosElegiveisAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Include(x => x.Turma)
            .Where(x => x.Status == StatusAlunoEnum.Ativo || x.Status == StatusAlunoEnum.Inativo)
            .OrderBy(x => x.NomeCompleto)
            .ToListAsync(cancellationToken);
    }

    public Task<Desligamento?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Desligamentos
            .AsNoTracking()
            .Include(x => x.Aluno)
            .ThenInclude(x => x!.Turma)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<decimal> CalcularPendenciasAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var hoje = clock.TodayDate;

        var totalPendencias = await dbContext.Mensalidades
            .Where(x => x.AlunoId == alunoId &&
                        (x.Status == StatusMensalidadeEnum.Atrasado ||
                         (x.Status == StatusMensalidadeEnum.Pendente && x.DataVencimento <= hoje)))
            .SumAsync(x => (decimal?)x.ValorFinal, cancellationToken) ?? 0m;

        return decimal.Round(totalPendencias, 2);
    }
}
