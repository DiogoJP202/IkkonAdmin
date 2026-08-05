using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Pagination;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoConquistaAdminService(
    ApplicationDbContext dbContext,
    IClock clock,
    IInsigniaRuleEvaluator ruleEvaluator) : IAreaAlunoConquistaAdminService
{
    public async Task<AreaAlunoConquistasAdminViewModel> ObterConquistasAsync(
        ConquistaAdminFilter filter,
        CancellationToken cancellationToken = default)
    {
        var insignias = await ListarInsigniasAsync(cancellationToken);
        return new AreaAlunoConquistasAdminViewModel
        {
            Filtro = filter,
            Alunos = await ListarAlunosOpcoesAsync(cancellationToken),
            Insignias = insignias,
            Categorias = insignias
                .Select(x => x.Categoria)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList(),
            Conquistas = await ListarConquistasAdminAsync(filter, cancellationToken)
        };
    }

    public Task<int> ContarConquistasConcedidasAsync(
        DateTime inicioUtc,
        DateTime fimUtc,
        CancellationToken cancellationToken = default)
    {
        return dbContext.AlunoInsignias
            .CountAsync(x => x.ConcedidaEmUtc >= inicioUtc && x.ConcedidaEmUtc < fimUtc, cancellationToken);
    }

    public async Task<OperationResult> CriarInsigniaAsync(
        InsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var ruleValidation = ruleEvaluator.ValidateRule(model.RegraAutomatica);
        if (!ruleValidation.Success)
        {
            return ruleValidation;
        }

        var nome = model.Nome.Trim();
        var existe = await dbContext.Insignias.AnyAsync(x => x.Nome == nome, cancellationToken);
        if (existe)
        {
            return OperationResult.Fail("Já existe uma insígnia com este nome.");
        }

        await dbContext.Insignias.AddAsync(new Insignia
        {
            Nome = nome,
            Descricao = LimparOpcional(model.Descricao),
            Icone = LimparOpcional(model.Icone),
            Categoria = LimparOpcional(model.Categoria),
            RegraAutomatica = LimparOpcional(model.RegraAutomatica),
            Ativa = model.Ativa
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Insígnia criada.");
    }

    public async Task<OperationResult> AtualizarInsigniaAsync(
        int id,
        InsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var ruleValidation = ruleEvaluator.ValidateRule(model.RegraAutomatica);
        if (!ruleValidation.Success)
        {
            return ruleValidation;
        }

        var insignia = await dbContext.Insignias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (insignia is null)
        {
            return OperationResult.Fail("Insígnia não encontrada.");
        }

        var nome = model.Nome.Trim();
        var existe = await dbContext.Insignias
            .AnyAsync(x => x.Id != id && x.Nome == nome, cancellationToken);

        if (existe)
        {
            return OperationResult.Fail("Já existe uma insígnia com este nome.");
        }

        insignia.Nome = nome;
        insignia.Descricao = LimparOpcional(model.Descricao);
        insignia.Icone = LimparOpcional(model.Icone);
        insignia.Categoria = LimparOpcional(model.Categoria);
        insignia.RegraAutomatica = LimparOpcional(model.RegraAutomatica);
        insignia.Ativa = model.Ativa;

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Insígnia atualizada.");
    }

    public async Task<OperationResult> ExcluirInsigniaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var insignia = await dbContext.Insignias
            .Include(x => x.Alunos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (insignia is null)
        {
            return OperationResult.Fail("Insígnia não encontrada.");
        }

        if (insignia.Alunos.Count > 0)
        {
            insignia.Ativa = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Ok("Insígnia desativada porque já foi atribuída.");
        }

        dbContext.Insignias.Remove(insignia);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Insígnia excluída.");
    }

    public async Task<OperationResult> AtribuirInsigniaAsync(
        AlunoInsigniaFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoExiste = await dbContext.Alunos.AnyAsync(x => x.Id == model.AlunoId, cancellationToken);
        var insigniaExiste = await dbContext.Insignias.AnyAsync(x => x.Id == model.InsigniaId && x.Ativa, cancellationToken);

        if (!alunoExiste || !insigniaExiste)
        {
            return OperationResult.Fail("Aluno ou insígnia inválida.");
        }

        var jaPossui = await dbContext.AlunoInsignias
            .AnyAsync(x => x.AlunoId == model.AlunoId && x.InsigniaId == model.InsigniaId, cancellationToken);

        if (jaPossui)
        {
            return OperationResult.Fail("Este aluno já possui esta insígnia.");
        }

        await dbContext.AlunoInsignias.AddAsync(new AlunoInsignia
        {
            AlunoId = model.AlunoId,
            InsigniaId = model.InsigniaId,
            Origem = InsigniaOrigemEnum.Manual,
            ConcedidaPorUsuarioId = usuarioId,
            ConcedidaEmUtc = clock.UtcNow,
            Observacao = LimparOpcional(model.Observacao)
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Insígnia atribuída ao aluno.");
    }

    public async Task<OperationResult> AtualizarAlunoInsigniaAsync(
        int id,
        AlunoInsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var conquista = await dbContext.AlunoInsignias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (conquista is null)
        {
            return OperationResult.Fail("Conquista não encontrada.");
        }

        var alunoExiste = await dbContext.Alunos.AnyAsync(x => x.Id == model.AlunoId, cancellationToken);
        var insigniaExiste = await dbContext.Insignias.AnyAsync(x => x.Id == model.InsigniaId && x.Ativa, cancellationToken);

        if (!alunoExiste || !insigniaExiste)
        {
            return OperationResult.Fail("Aluno ou insígnia inválida.");
        }

        var duplicada = await dbContext.AlunoInsignias
            .AnyAsync(x => x.Id != id && x.AlunoId == model.AlunoId && x.InsigniaId == model.InsigniaId, cancellationToken);

        if (duplicada)
        {
            return OperationResult.Fail("Este aluno já possui esta insígnia.");
        }

        conquista.AlunoId = model.AlunoId;
        conquista.InsigniaId = model.InsigniaId;
        conquista.Observacao = LimparOpcional(model.Observacao);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Conquista atualizada.");
    }

    public async Task<OperationResult> ExcluirAlunoInsigniaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var conquista = await dbContext.AlunoInsignias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (conquista is null)
        {
            return OperationResult.Fail("Conquista não encontrada.");
        }

        dbContext.AlunoInsignias.Remove(conquista);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Conquista removida do aluno.");
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

    private async Task<IReadOnlyCollection<AreaAlunoInsigniaItemViewModel>> ListarInsigniasAsync(CancellationToken cancellationToken)
    {
        var items = await dbContext.Insignias
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new AreaAlunoInsigniaItemViewModel
            {
                Id = x.Id,
                Nome = x.Nome,
                Descricao = x.Descricao,
                Icone = x.Icone,
                Categoria = x.Categoria,
                RegraAutomatica = x.RegraAutomatica,
                Ativa = x.Ativa
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.RegraAutomaticaValida = ruleEvaluator.ValidateRule(item.RegraAutomatica).Success;
        }

        return items;
    }

    private async Task<PagedResult<AreaAlunoConquistaAdminItemViewModel>> ListarConquistasAdminAsync(
        ConquistaAdminFilter filter,
        CancellationToken cancellationToken)
    {
        filter.Normalize();
        var query = dbContext.AlunoInsignias
            .AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.Insignia)
            .AsQueryable();

        if (filter.AlunoId.HasValue)
        {
            query = query.Where(x => x.AlunoId == filter.AlunoId.Value);
        }

        if (filter.InsigniaId.HasValue)
        {
            query = query.Where(x => x.InsigniaId == filter.InsigniaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Categoria))
        {
            var category = filter.Categoria.Trim();
            query = query.Where(x => x.Insignia != null && x.Insignia.Categoria == category);
        }

        if (filter.Origem.HasValue)
        {
            query = query.Where(x => x.Origem == filter.Origem.Value);
        }

        if (filter.Inicio.HasValue)
        {
            var start = filter.Inicio.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.ConcedidaEmUtc >= start);
        }

        if (filter.Fim.HasValue)
        {
            var endExclusive = filter.Fim.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(x => x.ConcedidaEmUtc < endExclusive);
        }

        query = filter.Sort switch
        {
            "data" => query.OrderBy(x => x.ConcedidaEmUtc).ThenBy(x => x.Id),
            "aluno" => query.OrderBy(x => x.Aluno!.NomeCompleto).ThenByDescending(x => x.ConcedidaEmUtc),
            "insignia" => query.OrderBy(x => x.Insignia!.Nome).ThenByDescending(x => x.ConcedidaEmUtc),
            _ => query.OrderByDescending(x => x.ConcedidaEmUtc).ThenByDescending(x => x.Id)
        };

        var paged = await query.ToPagedResultAsync(filter, cancellationToken);
        return paged.Map(x => new AreaAlunoConquistaAdminItemViewModel
            {
                Id = x.Id,
                AlunoId = x.AlunoId,
                Aluno = x.Aluno != null ? x.Aluno.NomeCompleto : $"Aluno #{x.AlunoId}",
                InsigniaId = x.InsigniaId,
                Insignia = x.Insignia != null ? x.Insignia.Nome : $"Insígnia #{x.InsigniaId}",
                ConcedidaEmUtc = x.ConcedidaEmUtc,
                Origem = x.Origem,
                Observacao = x.Observacao
            });
    }

    private static string? LimparOpcional(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}
