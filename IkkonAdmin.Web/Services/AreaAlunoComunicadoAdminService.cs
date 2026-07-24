using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoComunicadoAdminService(
    ApplicationDbContext dbContext,
    IClock clock) : IAreaAlunoComunicadoAdminService
{
    public async Task<AreaAlunoComunicadosAdminViewModel> ObterComunicadosAsync(CancellationToken cancellationToken = default)
    {
        return new AreaAlunoComunicadosAdminViewModel
        {
            Alunos = await ListarAlunosOpcoesAsync(cancellationToken),
            Turmas = await ListarTurmasOpcoesAsync(cancellationToken),
            Comunicados = await ListarComunicadosRecentesAsync(100, cancellationToken)
        };
    }

    public Task<int> ContarComunicadosAtivosAsync(CancellationToken cancellationToken = default)
    {
        var agoraUtc = clock.UtcNow;
        return dbContext.Comunicados.CountAsync(
            x => x.Ativo &&
                 x.PublicadoEmUtc <= agoraUtc &&
                 (!x.ExpiraEmUtc.HasValue || x.ExpiraEmUtc >= agoraUtc),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<AreaAlunoComunicadoAdminItemViewModel>> ListarComunicadosRecentesAsync(
        int limite,
        CancellationToken cancellationToken = default)
    {
        var comunicados = await dbContext.Comunicados
            .AsNoTracking()
            .Include(x => x.Alvos)
            .Include(x => x.Leituras)
            .OrderByDescending(x => x.Fixado)
            .ThenByDescending(x => x.PublicadoEmUtc)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return comunicados
            .Select(x =>
            {
                var alvo = x.Alvos.FirstOrDefault();
                return new AreaAlunoComunicadoAdminItemViewModel
                {
                    Id = x.Id,
                    Titulo = x.Titulo,
                    Conteudo = x.Conteudo,
                    Importante = x.Importante,
                    Fixado = x.Fixado,
                    Ativo = x.Ativo,
                    PublicadoEmUtc = x.PublicadoEmUtc,
                    ExpiraEmUtc = x.ExpiraEmUtc,
                    AlvoTipo = ObterAlvoTipo(alvo?.Todos == true, alvo?.AlunoId, alvo?.TurmaId),
                    AlunoId = alvo?.AlunoId,
                    TurmaId = alvo?.TurmaId,
                    Leituras = x.Leituras.Count
                };
            })
            .ToList();
    }

    public async Task<OperationResult> CriarComunicadoAsync(
        ComunicadoFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alvoValidado = await ValidarAlvoAsync(model.AlvoTipo, model.AlunoId, model.TurmaId, cancellationToken);
        if (!alvoValidado.Success)
        {
            return alvoValidado;
        }

        var comunicado = new Comunicado
        {
            Titulo = model.Titulo.Trim(),
            Conteudo = model.Conteudo.Trim(),
            Importante = model.Importante,
            Fixado = model.Fixado,
            PublicadoEmUtc = model.PublicadoEmUtc,
            ExpiraEmUtc = model.ExpiraEmUtc,
            Ativo = true,
            CriadoPorUsuarioId = usuarioId
        };

        comunicado.Alvos.Add(CriarComunicadoAlvo(model.AlvoTipo, model.AlunoId, model.TurmaId));
        await dbContext.Comunicados.AddAsync(comunicado, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Comunicado publicado.");
    }

    public async Task<OperationResult> AtualizarComunicadoAsync(
        int id,
        ComunicadoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var alvoValidado = await ValidarAlvoAsync(model.AlvoTipo, model.AlunoId, model.TurmaId, cancellationToken);
        if (!alvoValidado.Success)
        {
            return alvoValidado;
        }

        var comunicado = await dbContext.Comunicados
            .Include(x => x.Alvos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (comunicado is null)
        {
            return OperationResult.Fail("Comunicado não encontrado.");
        }

        comunicado.Titulo = model.Titulo.Trim();
        comunicado.Conteudo = model.Conteudo.Trim();
        comunicado.Importante = model.Importante;
        comunicado.Fixado = model.Fixado;
        comunicado.PublicadoEmUtc = model.PublicadoEmUtc;
        comunicado.ExpiraEmUtc = model.ExpiraEmUtc;
        comunicado.Ativo = true;

        dbContext.ComunicadosAlvos.RemoveRange(comunicado.Alvos);
        comunicado.Alvos.Add(CriarComunicadoAlvo(model.AlvoTipo, model.AlunoId, model.TurmaId));

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Comunicado atualizado.");
    }

    public async Task<OperationResult> ExcluirComunicadoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var comunicado = await dbContext.Comunicados
            .Include(x => x.Alvos)
            .Include(x => x.Leituras)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (comunicado is null)
        {
            return OperationResult.Fail("Comunicado não encontrado.");
        }

        if (comunicado.Leituras.Count > 0)
        {
            comunicado.Ativo = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Ok("Comunicado desativado porque já possui leituras.");
        }

        dbContext.ComunicadosAlvos.RemoveRange(comunicado.Alvos);
        dbContext.Comunicados.Remove(comunicado);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Comunicado excluído.");
    }

    private async Task<IReadOnlyCollection<AreaAlunoOpcaoViewModel>> ListarAlunosOpcoesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Status != StatusAlunoEnum.Desligado)
            .OrderBy(x => x.NomeCompleto)
            .Select(x => new AreaAlunoOpcaoViewModel
            {
                Id = x.Id,
                Nome = x.NomeCompleto
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoOpcaoViewModel>> ListarTurmasOpcoesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Turmas
            .AsNoTracking()
            .Where(x => x.Ativa)
            .OrderBy(x => x.Nome)
            .Select(x => new AreaAlunoOpcaoViewModel
            {
                Id = x.Id,
                Nome = x.Nome
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<OperationResult> ValidarAlvoAsync(
        ComunicadoAlvoTipoEnum alvoTipo,
        int? alunoId,
        int? turmaId,
        CancellationToken cancellationToken)
    {
        if (alvoTipo == ComunicadoAlvoTipoEnum.Todos)
        {
            return OperationResult.Ok("Alvo valido.");
        }

        if (alvoTipo == ComunicadoAlvoTipoEnum.Aluno)
        {
            var alunoExiste = alunoId.HasValue &&
                              await dbContext.Alunos.AnyAsync(x => x.Id == alunoId.Value, cancellationToken);

            return alunoExiste
                ? OperationResult.Ok("Alvo valido.")
                : OperationResult.Fail("Selecione um aluno valido.");
        }

        var turmaExiste = turmaId.HasValue &&
                          await dbContext.Turmas.AnyAsync(x => x.Id == turmaId.Value, cancellationToken);

        return turmaExiste
            ? OperationResult.Ok("Alvo valido.")
            : OperationResult.Fail("Selecione uma turma válida.");
    }

    private static ComunicadoAlvo CriarComunicadoAlvo(
        ComunicadoAlvoTipoEnum alvoTipo,
        int? alunoId,
        int? turmaId)
    {
        return new ComunicadoAlvo
        {
            Todos = alvoTipo == ComunicadoAlvoTipoEnum.Todos,
            AlunoId = alvoTipo == ComunicadoAlvoTipoEnum.Aluno ? alunoId : null,
            TurmaId = alvoTipo == ComunicadoAlvoTipoEnum.Turma ? turmaId : null
        };
    }

    private static ComunicadoAlvoTipoEnum ObterAlvoTipo(
        bool todos,
        int? alunoId,
        int? turmaId)
    {
        if (todos)
        {
            return ComunicadoAlvoTipoEnum.Todos;
        }

        if (alunoId.HasValue)
        {
            return ComunicadoAlvoTipoEnum.Aluno;
        }

        if (turmaId.HasValue)
        {
            return ComunicadoAlvoTipoEnum.Turma;
        }

        return ComunicadoAlvoTipoEnum.Todos;
    }
}
