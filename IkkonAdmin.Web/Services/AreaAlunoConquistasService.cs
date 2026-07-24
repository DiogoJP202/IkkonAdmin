using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoConquistasService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAreaAlunoContextService contextService) : IAreaAlunoConquistasService
{
    public async Task<AreaAlunoConquistasViewModel?> ObterConquistasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await contextService.ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        await GarantirConquistasAutomaticasAsync(alunoId.Value, cancellationToken);

        return new AreaAlunoConquistasViewModel
        {
            Conquistas = await ListarConquistasAsync(alunoId.Value, 100, cancellationToken)
        };
    }

    public async Task<List<AreaAlunoConquistaItemViewModel>> ListarConquistasAsync(
        int alunoId,
        int limite,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AlunoInsignias
            .AsNoTracking()
            .Include(x => x.Insignia)
            .Where(x => x.AlunoId == alunoId && x.Insignia != null && x.Insignia.Ativa)
            .OrderByDescending(x => x.ConcedidaEmUtc)
            .Take(limite)
            .Select(x => new AreaAlunoConquistaItemViewModel
            {
                Id = x.Id,
                Nome = x.Insignia!.Nome,
                Descricao = x.Insignia.Descricao,
                Icone = x.Insignia.Icone,
                Categoria = x.Insignia.Categoria,
                ConcedidaEmUtc = x.ConcedidaEmUtc,
                Origem = x.Origem,
                Observacao = x.Observacao
            })
            .ToListAsync(cancellationToken);
    }

    public async Task GarantirConquistasAutomaticasAsync(
        int alunoId,
        CancellationToken cancellationToken = default)
    {
        var aluno = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId)
            .Select(x => new { x.Id, x.DataEntrada })
            .FirstOrDefaultAsync(cancellationToken);

        if (aluno is null)
        {
            return;
        }

        var hoje = clock.TodayDate;
        var anos = hoje.Year - aluno.DataEntrada.Year;
        if (hoje < aluno.DataEntrada.AddYears(Math.Max(anos, 0)))
        {
            anos--;
        }

        if (anos >= 1)
        {
            await GarantirInsigniaAutomaticaAsync(
                aluno.Id,
                "1 ano de jornada",
                "Primeiro ano de participação na escola.",
                "Tempo",
                "tempo-1-ano",
                cancellationToken);
        }

        var possuiGraduacaoAprovada = await dbContext.Graduacoes
            .AsNoTracking()
            .AnyAsync(x => x.AlunoId == aluno.Id && x.ResultadoAprovado, cancellationToken);

        if (possuiGraduacaoAprovada)
        {
            await GarantirInsigniaAutomaticaAsync(
                aluno.Id,
                "Graduação conquistada",
                "Resultado aprovado em exame de graduação.",
                "Evolução",
                "graduacao-aprovada",
                cancellationToken);
        }
    }

    private async Task GarantirInsigniaAutomaticaAsync(
        int alunoId,
        string nome,
        string descricao,
        string categoria,
        string regra,
        CancellationToken cancellationToken)
    {
        var insignia = await dbContext.Insignias
            .FirstOrDefaultAsync(x => x.RegraAutomatica == regra, cancellationToken);

        if (insignia is null)
        {
            insignia = new Insignia
            {
                Nome = nome,
                Descricao = descricao,
                Categoria = categoria,
                Icone = "star",
                Ativa = true,
                RegraAutomatica = regra
            };

            await dbContext.Insignias.AddAsync(insignia, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var jaPossui = await dbContext.AlunoInsignias
            .AnyAsync(x => x.AlunoId == alunoId && x.InsigniaId == insignia.Id, cancellationToken);

        if (jaPossui)
        {
            return;
        }

        await dbContext.AlunoInsignias.AddAsync(new AlunoInsignia
        {
            AlunoId = alunoId,
            InsigniaId = insignia.Id,
            Origem = InsigniaOrigemEnum.Automatica,
            ConcedidaEmUtc = clock.UtcNow
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
