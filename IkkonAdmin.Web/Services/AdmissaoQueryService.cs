using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AdmissaoQueryService(ApplicationDbContext dbContext) : IAdmissaoQueryService
{
    public async Task<IReadOnlyList<Admissao>> ListarAsync(
        string? busca = null,
        StatusAdmissaoEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Admissoes
            .AsNoTracking()
            .Include(x => x.Aluno)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaTexto = busca.Trim();
            query = query.Where(x =>
                x.NomeInteressado.Contains(buscaTexto) ||
                (x.Aluno != null && x.Aluno.NomeCompleto.Contains(buscaTexto)));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.DataAulaExperimental)
            .ThenBy(x => x.NomeInteressado)
            .ToListAsync(cancellationToken);
    }

    public Task<Admissao?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Admissoes
            .AsNoTracking()
            .Include(x => x.Aluno)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Turma>> ListarTurmasAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Turmas
            .AsNoTracking()
            .Where(x => x.Ativa)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);
    }
}
