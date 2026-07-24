using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class GraduacaoQueryService(ApplicationDbContext dbContext) : IGraduacaoQueryService
{
    public async Task<IReadOnlyList<Graduacao>> ListarAsync(
        string? busca = null,
        bool? somenteAprovados = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Graduacoes
            .AsNoTracking()
            .Include(x => x.Aluno)
            .ThenInclude(x => x!.Turma)
            .Include(x => x.ExameGraduacao)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaTexto = busca.Trim();
            query = query.Where(x =>
                x.Aluno != null &&
                (x.Aluno.NomeCompleto.Contains(buscaTexto) ||
                 x.Aluno.CPF.Contains(buscaTexto)));
        }

        if (somenteAprovados.HasValue)
        {
            query = query.Where(x => x.ResultadoAprovado == somenteAprovados.Value);
        }

        return await query
            .OrderByDescending(x => x.DataResultado)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Graduacao>> ListarHistoricoAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Graduacoes
            .AsNoTracking()
            .Include(x => x.ExameGraduacao)
            .Where(x => x.AlunoId == alunoId)
            .OrderByDescending(x => x.DataResultado)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Aluno>> ListarAlunosAptosAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Include(x => x.Turma)
            .Where(x => x.Status == StatusAlunoEnum.Ativo)
            .OrderBy(x => x.NomeCompleto)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExameGraduacao>> ListarExamesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.ExamesGraduacao
            .AsNoTracking()
            .Include(x => x.Graduacoes)
            .OrderByDescending(x => x.DataExame)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Graduacao?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Graduacoes
            .AsNoTracking()
            .Include(x => x.Aluno)
            .ThenInclude(x => x!.Turma)
            .Include(x => x.ExameGraduacao)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<NivelGraduacaoEnum> ObterNivelAtualAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var nivelAtual = await dbContext.Graduacoes
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId && x.ResultadoAprovado)
            .OrderByDescending(x => x.DataResultado)
            .ThenByDescending(x => x.Id)
            .Select(x => (NivelGraduacaoEnum?)(x.NivelNovo ?? x.NivelAnterior))
            .FirstOrDefaultAsync(cancellationToken);

        return nivelAtual ?? NivelGraduacaoEnum.Iniciante;
    }
}
