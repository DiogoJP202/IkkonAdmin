using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AlunoQueryService(ApplicationDbContext dbContext) : IAlunoQueryService
{
    public async Task<(IReadOnlyList<Aluno> Itens, int TotalRegistros)> ListarAsync(
        string? busca = null,
        StatusAlunoEnum? status = null,
        int? turmaId = null,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = Math.Clamp(tamanhoPagina, 5, 100);

        var query = dbContext.Alunos
            .AsNoTracking()
            .Include(x => x.Turma)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaTexto = busca.Trim();
            var buscaDigitos = SomenteDigitos(buscaTexto);
            var temBuscaDigitos = !string.IsNullOrWhiteSpace(buscaDigitos);

            query = query.Where(x =>
                x.NomeCompleto.Contains(buscaTexto) ||
                x.CPF.Contains(buscaTexto) ||
                (x.Celular != null && x.Celular.Contains(buscaTexto)) ||
                (temBuscaDigitos &&
                 (x.CPF.Replace(".", string.Empty).Replace("-", string.Empty).Contains(buscaDigitos) ||
                  (x.Celular != null &&
                   x.Celular
                       .Replace("(", string.Empty)
                       .Replace(")", string.Empty)
                       .Replace("-", string.Empty)
                       .Replace(" ", string.Empty)
                       .Contains(buscaDigitos)))));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (turmaId.HasValue)
        {
            query = query.Where(x =>
                x.TurmaId == turmaId.Value ||
                x.AlunoTurmas.Any(t => t.TurmaId == turmaId.Value));
        }

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderBy(x => x.NomeCompleto)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return (itens, totalRegistros);
    }

    public async Task<IReadOnlyList<Turma>> ListarTurmasAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Turmas
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);
    }

    public Task<Aluno?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Alunos
            .AsNoTracking()
            .Include(x => x.Turma)
            .Include(x => x.Mensalidades)
            .Include(x => x.Pagamentos)
            .Include(x => x.Historicos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExisteCpfAsync(string cpf, int? ignorarAlunoId = null, CancellationToken cancellationToken = default)
    {
        var cpfNormalizado = SomenteDigitos(cpf);

        if (string.IsNullOrWhiteSpace(cpfNormalizado))
        {
            return Task.FromResult(false);
        }

        var query = dbContext.Alunos.AsNoTracking();

        if (ignorarAlunoId.HasValue)
        {
            query = query.Where(x => x.Id != ignorarAlunoId.Value);
        }

        return query.AnyAsync(x =>
            x.CPF == cpfNormalizado ||
            x.CPF.Replace(".", string.Empty).Replace("-", string.Empty) == cpfNormalizado, cancellationToken);
    }

    private static string SomenteDigitos(string valor)
    {
        return new string(valor.Where(char.IsDigit).ToArray());
    }
}
