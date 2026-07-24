using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class TurmaQueryService(ApplicationDbContext dbContext) : ITurmaQueryService
{
    public async Task<IReadOnlyList<Turma>> ListarAsync(
        string? busca = null,
        bool? ativa = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Turmas
            .AsNoTracking()
            .Include(x => x.AlunoTurmas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var textoBusca = busca.Trim();
            query = query.Where(x =>
                x.Nome.Contains(textoBusca) ||
                x.Modalidade.Contains(textoBusca) ||
                (x.Horario != null && x.Horario.Contains(textoBusca)));
        }

        if (ativa.HasValue)
        {
            query = query.Where(x => x.Ativa == ativa.Value);
        }

        return await query
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Turma?> ObterComAlunosAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Turmas
            .AsNoTracking()
            .Include(x => x.AlunoTurmas)
            .ThenInclude(x => x.Aluno)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Aluno>> ListarAlunosVinculaveisAsync(
        int? turmaIdAtual = null,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Include(x => x.Turma)
            .Include(x => x.AlunoTurmas)
            .ThenInclude(x => x.Turma)
            .Where(x => x.Status != StatusAlunoEnum.Desligado || x.AlunoTurmas.Any(t => t.TurmaId == turmaIdAtual))
            .OrderBy(x => x.NomeCompleto)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> ExisteNomeAsync(
        string nome,
        int? ignorarTurmaId = null,
        CancellationToken cancellationToken = default)
    {
        var nomeNormalizado = nome.Trim();
        var query = dbContext.Turmas.AsNoTracking();

        if (ignorarTurmaId.HasValue)
        {
            query = query.Where(x => x.Id != ignorarTurmaId.Value);
        }

        return query.AnyAsync(x => x.Nome == nomeNormalizado, cancellationToken);
    }
}
