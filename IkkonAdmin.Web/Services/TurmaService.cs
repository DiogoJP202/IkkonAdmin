using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class TurmaService(ApplicationDbContext dbContext) : ITurmaService
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

    public async Task<IReadOnlyList<Aluno>> ListarAlunosVinculaveisAsync(int? turmaIdAtual = null, CancellationToken cancellationToken = default)
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

    public Task<bool> ExisteNomeAsync(string nome, int? ignorarTurmaId = null, CancellationToken cancellationToken = default)
    {
        var nomeNormalizado = nome.Trim();
        var query = dbContext.Turmas.AsQueryable();

        if (ignorarTurmaId.HasValue)
        {
            query = query.Where(x => x.Id != ignorarTurmaId.Value);
        }

        return query.AnyAsync(x => x.Nome == nomeNormalizado, cancellationToken);
    }

    public async Task<int> CriarAsync(Turma turma, IReadOnlyCollection<int> alunosIds, CancellationToken cancellationToken = default)
    {
        await dbContext.Turmas.AddAsync(turma, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var ids = alunosIds.Distinct().ToArray();
        if (ids.Length > 0)
        {
            var alunosSelecionados = await dbContext.Alunos
                .Include(x => x.AlunoTurmas)
                .Where(x => ids.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var aluno in alunosSelecionados)
            {
                if (!aluno.AlunoTurmas.Any(x => x.TurmaId == turma.Id))
                {
                    aluno.AlunoTurmas.Add(new AlunoTurma
                    {
                        AlunoId = aluno.Id,
                        TurmaId = turma.Id,
                        DataVinculo = DateTime.UtcNow
                    });
                }

                // Mantem compatibilidade com telas antigas que ainda usam TurmaId como referencia principal.
                if (!aluno.TurmaId.HasValue)
                {
                    aluno.TurmaId = turma.Id;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return turma.Id;
    }

    public async Task<bool> AtualizarAsync(
        int id,
        Turma turmaAtualizada,
        IReadOnlyCollection<int> alunosIds,
        CancellationToken cancellationToken = default)
    {
        var turma = await dbContext.Turmas
            .Include(x => x.AlunoTurmas)
            .ThenInclude(x => x.Aluno)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (turma is null)
        {
            return false;
        }

        turma.Nome = turmaAtualizada.Nome;
        turma.Modalidade = turmaAtualizada.Modalidade;
        turma.Horario = turmaAtualizada.Horario;
        turma.Ativa = turmaAtualizada.Ativa;
        turma.Observacoes = turmaAtualizada.Observacoes;

        var idsSelecionados = alunosIds.Distinct().ToHashSet();
        var idsAtuais = turma.AlunoTurmas.Select(x => x.AlunoId).ToHashSet();

        var vinculosRemover = turma.AlunoTurmas
            .Where(x => !idsSelecionados.Contains(x.AlunoId))
            .ToList();

        if (vinculosRemover.Count > 0)
        {
            dbContext.AlunosTurmas.RemoveRange(vinculosRemover);
        }

        var idsAdicionar = idsSelecionados.Except(idsAtuais).ToArray();
        if (idsAdicionar.Length > 0)
        {
            var alunosAdicionar = await dbContext.Alunos
                .Include(x => x.AlunoTurmas)
                .Where(x => idsAdicionar.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var aluno in alunosAdicionar)
            {
                if (!aluno.AlunoTurmas.Any(x => x.TurmaId == turma.Id))
                {
                    aluno.AlunoTurmas.Add(new AlunoTurma
                    {
                        AlunoId = aluno.Id,
                        TurmaId = turma.Id,
                        DataVinculo = DateTime.UtcNow
                    });
                }

                if (!aluno.TurmaId.HasValue)
                {
                    aluno.TurmaId = turma.Id;
                }
            }
        }

        // Reconciliacao de TurmaId legado para quem perdeu a turma principal nesta atualizacao.
        var idsAfetados = vinculosRemover
            .Select(x => x.AlunoId)
            .Distinct()
            .ToArray();

        if (idsAfetados.Length > 0)
        {
            var alunosAfetados = await dbContext.Alunos
                .Include(x => x.AlunoTurmas)
                .Where(x => idsAfetados.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var aluno in alunosAfetados)
            {
                if (aluno.TurmaId != turma.Id)
                {
                    continue;
                }

                aluno.TurmaId = aluno.AlunoTurmas
                    .Where(x => x.TurmaId != turma.Id)
                    .Select(x => (int?)x.TurmaId)
                    .FirstOrDefault();
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
