using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AlunoService(ApplicationDbContext dbContext) : IAlunoService
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

    public Task<Aluno?> ObterParaEdicaoAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Alunos
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

        var query = dbContext.Alunos.AsQueryable();

        if (ignorarAlunoId.HasValue)
        {
            query = query.Where(x => x.Id != ignorarAlunoId.Value);
        }

        return query.AnyAsync(x =>
            x.CPF == cpfNormalizado ||
            x.CPF.Replace(".", string.Empty).Replace("-", string.Empty) == cpfNormalizado, cancellationToken);
    }

    public async Task AdicionarAsync(Aluno aluno, CancellationToken cancellationToken = default)
    {
        if (aluno.TurmaId.HasValue && !aluno.AlunoTurmas.Any(x => x.TurmaId == aluno.TurmaId.Value))
        {
            aluno.AlunoTurmas.Add(new AlunoTurma
            {
                TurmaId = aluno.TurmaId.Value,
                DataVinculo = DateTime.UtcNow
            });
        }

        await dbContext.Alunos.AddAsync(aluno, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        var alunosComTurmaAlterada = dbContext.ChangeTracker
            .Entries<Aluno>()
            .Where(x => x.State is EntityState.Modified or EntityState.Added)
            .Select(x => x.Entity)
            .Where(x => x.TurmaId.HasValue)
            .ToList();

        foreach (var aluno in alunosComTurmaAlterada)
        {
            var turmaId = aluno.TurmaId!.Value;
            var possuiVinculo = await dbContext.AlunosTurmas
                .AnyAsync(x => x.AlunoId == aluno.Id && x.TurmaId == turmaId, cancellationToken);

            if (!possuiVinculo)
            {
                dbContext.AlunosTurmas.Add(new AlunoTurma
                {
                    AlunoId = aluno.Id,
                    TurmaId = turmaId,
                    DataVinculo = DateTime.UtcNow
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> AlterarStatusAsync(int id, StatusAlunoEnum status, CancellationToken cancellationToken = default)
    {
        var aluno = await dbContext.Alunos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (aluno is null)
        {
            return false;
        }

        aluno.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string SomenteDigitos(string valor)
    {
        return new string(valor.Where(char.IsDigit).ToArray());
    }
}
