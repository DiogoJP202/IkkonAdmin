using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoPerfilService(
    ApplicationDbContext dbContext,
    IAreaAlunoContextService contextService) : IAreaAlunoPerfilService
{
    public async Task<AreaAlunoPerfilViewModel?> ObterPerfilAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await contextService.ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        return await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId.Value)
            .Select(x => new AreaAlunoPerfilViewModel
            {
                NomeCompleto = x.NomeCompleto,
                CPF = x.CPF,
                RG = x.RG,
                DataNascimento = x.DataNascimento,
                Email = x.Email,
                Celular = x.Celular,
                Endereco = x.Endereco,
                ContatoEmergencia = x.ContatoEmergencia,
                DataEntrada = x.DataEntrada,
                Status = x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AreaAlunoPerfilBase?> ObterPerfilBaseAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId)
            .Select(x => new AreaAlunoPerfilBase
            {
                AlunoId = x.Id,
                NomeCompleto = x.NomeCompleto,
                Email = x.Email,
                Celular = x.Celular,
                Status = x.Status,
                TurmaPrincipal = x.Turma != null ? x.Turma.Nome : null,
                DataEntrada = x.DataEntrada
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
