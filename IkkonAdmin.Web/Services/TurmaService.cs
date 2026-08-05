using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class TurmaService(
    ApplicationDbContext dbContext,
    IClock clock,
    ITurmaQueryService queryService) : ITurmaService
{
    public async Task<OperationResult<int>> CriarAsync(
        Turma turma,
        IReadOnlyCollection<int> alunosIds,
        CancellationToken cancellationToken = default)
    {
        NormalizarTurma(turma);

        if (await queryService.ExisteNomeAsync(turma.Nome, cancellationToken: cancellationToken))
        {
            return OperationResult<int>.Fail("Já existe uma turma com esse nome.", nameof(Turma.Nome));
        }

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
                        DataVinculo = clock.UtcNow
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

        return OperationResult<int>.Ok(turma.Id, "Turma criada com sucesso.");
    }

    public async Task<OperationResult> AtualizarAsync(
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
            return OperationResult.NotFound("Turma não encontrada.");
        }

        NormalizarTurma(turmaAtualizada);

        if (await queryService.ExisteNomeAsync(turmaAtualizada.Nome, id, cancellationToken))
        {
            return OperationResult.Fail("Já existe uma turma com esse nome.", nameof(Turma.Nome));
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
                        DataVinculo = clock.UtcNow
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
        return OperationResult.Ok("Turma atualizada com sucesso.");
    }

    private static void NormalizarTurma(Turma turma)
    {
        turma.Nome = turma.Nome.Trim();
        turma.Modalidade = turma.Modalidade.Trim();
        turma.Horario = LimparOpcional(turma.Horario);
        turma.Observacoes = LimparOpcional(turma.Observacoes);
    }

    private static string? LimparOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
