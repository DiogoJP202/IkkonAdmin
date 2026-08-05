using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Pagination;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoEventoAdminService(
    ApplicationDbContext dbContext,
    IClock clock) : IAreaAlunoEventoAdminService
{
    public async Task<AreaAlunoEventosAdminViewModel> ObterEventosAsync(
        EventoAdminFilter filter,
        CancellationToken cancellationToken = default)
    {
        return new AreaAlunoEventosAdminViewModel
        {
            Filtro = filter,
            Alunos = await ListarAlunosOpcoesAsync(cancellationToken),
            Turmas = await ListarTurmasOpcoesAsync(cancellationToken),
            Eventos = await ListarEventosPaginadosAsync(filter, cancellationToken)
        };
    }

    public Task<int> ContarEventosProximosAsync(CancellationToken cancellationToken = default)
    {
        var hoje = clock.Today;
        return dbContext.EventosAlunoPortal.CountAsync(x => x.Ativo && x.Fim >= hoje, cancellationToken);
    }

    public async Task<OperationResult> CriarEventoAsync(
        EventoAlunoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Fim <= model.Inicio)
        {
            return OperationResult.Fail("O fim do evento deve ser posterior ao inicio.");
        }

        var alvoValidado = await ValidarAlvoAsync(model.AlvoTipo, model.AlunoId, model.TurmaId, cancellationToken);
        if (!alvoValidado.Success)
        {
            return alvoValidado;
        }

        var evento = new EventoAlunoPortal
        {
            Titulo = model.Titulo.Trim(),
            Descricao = LimparOpcional(model.Descricao),
            Inicio = model.Inicio,
            Fim = model.Fim,
            Local = LimparOpcional(model.Local),
            Tipo = model.Tipo,
            Importante = model.Importante,
            Ativo = true,
            GoogleEventoId = LimparOpcional(model.GoogleEventoId)
        };

        evento.Alvos.Add(CriarEventoAlvo(model.AlvoTipo, model.AlunoId, model.TurmaId));
        await dbContext.EventosAlunoPortal.AddAsync(evento, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult.Ok("Evento cadastrado.");
    }

    public async Task<OperationResult> AtualizarEventoAsync(
        int id,
        EventoAlunoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Fim <= model.Inicio)
        {
            return OperationResult.Fail("O fim do evento deve ser posterior ao inicio.");
        }

        var alvoValidado = await ValidarAlvoAsync(model.AlvoTipo, model.AlunoId, model.TurmaId, cancellationToken);
        if (!alvoValidado.Success)
        {
            return alvoValidado;
        }

        var evento = await dbContext.EventosAlunoPortal
            .Include(x => x.Alvos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (evento is null)
        {
            return OperationResult.Fail("Evento não encontrado.");
        }

        evento.Titulo = model.Titulo.Trim();
        evento.Descricao = LimparOpcional(model.Descricao);
        evento.Inicio = model.Inicio;
        evento.Fim = model.Fim;
        evento.Local = LimparOpcional(model.Local);
        evento.Tipo = model.Tipo;
        evento.Importante = model.Importante;
        evento.GoogleEventoId = LimparOpcional(model.GoogleEventoId);
        evento.Ativo = true;

        dbContext.EventosAlunoPortalAlvos.RemoveRange(evento.Alvos);
        evento.Alvos.Add(CriarEventoAlvo(model.AlvoTipo, model.AlunoId, model.TurmaId));

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Evento atualizado.");
    }

    public async Task<OperationResult> ExcluirEventoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var evento = await dbContext.EventosAlunoPortal
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (evento is null)
        {
            return OperationResult.Fail("Evento não encontrado.");
        }

        evento.Ativo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Evento desativado.");
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

    private async Task<PagedResult<AreaAlunoEventoAdminItemViewModel>> ListarEventosPaginadosAsync(
        EventoAdminFilter filter,
        CancellationToken cancellationToken)
    {
        filter.Normalize();
        var query = dbContext.EventosAlunoPortal
            .AsNoTracking()
            .Include(x => x.Alvos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Busca))
        {
            var search = filter.Busca.Trim();
            query = query.Where(x => x.Titulo.Contains(search) ||
                                     (x.Descricao != null && x.Descricao.Contains(search)) ||
                                     (x.Local != null && x.Local.Contains(search)));
        }

        if (filter.Tipo.HasValue)
        {
            query = query.Where(x => x.Tipo == filter.Tipo.Value);
        }

        if (filter.Publico.HasValue)
        {
            query = filter.Publico.Value switch
            {
                ComunicadoAlvoTipoEnum.Aluno => query.Where(x => x.Alvos.Any(a => a.AlunoId.HasValue)),
                ComunicadoAlvoTipoEnum.Turma => query.Where(x => x.Alvos.Any(a => a.TurmaId.HasValue)),
                _ => query.Where(x => x.Alvos.Any(a => a.Todos))
            };
        }

        if (filter.Inicio.HasValue)
        {
            var start = filter.Inicio.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.Inicio >= start);
        }

        if (filter.Fim.HasValue)
        {
            var endExclusive = filter.Fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.Inicio < endExclusive);
        }

        if (filter.Importante.HasValue)
        {
            query = query.Where(x => x.Importante == filter.Importante.Value);
        }

        if (filter.Proximo.HasValue)
        {
            var now = clock.Now;
            query = filter.Proximo.Value
                ? query.Where(x => x.Fim >= now)
                : query.Where(x => x.Fim < now);
        }

        query = filter.Sort switch
        {
            "data-desc" => query.OrderByDescending(x => x.Inicio).ThenByDescending(x => x.Id),
            "titulo" => query.OrderBy(x => x.Titulo).ThenBy(x => x.Inicio),
            "tipo" => query.OrderBy(x => x.Tipo).ThenBy(x => x.Inicio),
            _ => query.OrderBy(x => x.Inicio).ThenBy(x => x.Id)
        };

        var eventos = await query.ToPagedResultAsync(filter, cancellationToken);

        return eventos.Map(x =>
            {
                var alvo = x.Alvos.FirstOrDefault();
                return new AreaAlunoEventoAdminItemViewModel
                {
                    Id = x.Id,
                    Titulo = x.Titulo,
                    Descricao = x.Descricao,
                    Inicio = x.Inicio,
                    Fim = x.Fim,
                    Local = x.Local,
                    Tipo = x.Tipo,
                    Importante = x.Importante,
                    Ativo = x.Ativo,
                    GoogleEventoId = x.GoogleEventoId,
                    AlvoTipo = ObterAlvoTipo(alvo?.Todos == true, alvo?.AlunoId, alvo?.TurmaId),
                    AlunoId = alvo?.AlunoId,
                    TurmaId = alvo?.TurmaId
                };
            });
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

    private static EventoAlunoPortalAlvo CriarEventoAlvo(
        ComunicadoAlvoTipoEnum alvoTipo,
        int? alunoId,
        int? turmaId)
    {
        return new EventoAlunoPortalAlvo
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

    private static string? LimparOpcional(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}
