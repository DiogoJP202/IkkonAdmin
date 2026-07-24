using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoContextService(ApplicationDbContext dbContext) : IAreaAlunoContextService
{
    public async Task<AreaAlunoPortalContexto?> ObterContextoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.UsuariosSistema
            .AsNoTracking()
            .Where(x => x.Id == usuarioId && x.Ativo && x.TipoAcesso == TipoAcessoEnum.Aluno)
            .Select(x => new
            {
                x.AlunoId,
                x.FotoPerfilUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (usuario?.AlunoId is not int alunoId)
        {
            return null;
        }

        var turmaIds = await dbContext.AlunosTurmas
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId)
            .Select(x => x.TurmaId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var turmaPrincipalId = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId && x.TurmaId.HasValue)
            .Select(x => x.TurmaId!.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (turmaPrincipalId > 0 && !turmaIds.Contains(turmaPrincipalId))
        {
            turmaIds.Add(turmaPrincipalId);
        }

        return new AreaAlunoPortalContexto(alunoId, turmaIds, usuario.FotoPerfilUrl);
    }

    public async Task<int?> ObterAlunoIdVinculadoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.UsuariosSistema
            .AsNoTracking()
            .Where(x => x.Id == usuarioId && x.Ativo && x.TipoAcesso == TipoAcessoEnum.Aluno)
            .Select(x => x.AlunoId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
