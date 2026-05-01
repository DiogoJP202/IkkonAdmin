using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class GraduacaoService(ApplicationDbContext dbContext) : IGraduacaoService
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

    public async Task<IReadOnlyList<Graduacao>> ListarHistoricoAlunoAsync(int alunoId, CancellationToken cancellationToken = default)
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

    public async Task<int> CriarExameAsync(ExameGraduacao exame, CancellationToken cancellationToken = default)
    {
        exame.Local = LimparOpcional(exame.Local);
        exame.Observacoes = LimparOpcional(exame.Observacoes);

        await dbContext.ExamesGraduacao.AddAsync(exame, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return exame.Id;
    }

    public async Task<GraduacaoRegistroResultado> RegistrarResultadoAsync(
        GraduacaoRegistroInput input,
        CancellationToken cancellationToken = default)
    {
        var aluno = await dbContext.Alunos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == input.AlunoId, cancellationToken);

        if (aluno is null)
        {
            return new GraduacaoRegistroResultado { Erro = "Aluno nao encontrado." };
        }

        if (aluno.Status != StatusAlunoEnum.Ativo)
        {
            return new GraduacaoRegistroResultado { Erro = "Somente alunos ativos podem receber registro de graduacao." };
        }

        if (input.ResultadoAprovado && !input.NivelNovo.HasValue)
        {
            return new GraduacaoRegistroResultado { Erro = "Informe o nivel novo para registrar resultado aprovado." };
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        ExameGraduacao? exameNovo = null;
        int? exameGraduacaoId = input.ExameGraduacaoId;

        if (exameGraduacaoId.HasValue)
        {
            var exameExiste = await dbContext.ExamesGraduacao
                .AsNoTracking()
                .AnyAsync(x => x.Id == exameGraduacaoId.Value, cancellationToken);

            if (!exameExiste)
            {
                return new GraduacaoRegistroResultado { Erro = "Exame informado nao encontrado." };
            }
        }
        else if (input.DataExameNovo.HasValue)
        {
            exameNovo = new ExameGraduacao
            {
                DataExame = input.DataExameNovo.Value,
                Local = LimparOpcional(input.LocalExameNovo),
                NivelPretendido = input.NivelPretendidoExameNovo
            };

            await dbContext.ExamesGraduacao.AddAsync(exameNovo, cancellationToken);
        }

        var nivelAnterior = await ObterNivelAtualAsync(input.AlunoId, cancellationToken);

        var nivelNovo = input.ResultadoAprovado
            ? input.NivelNovo
            : null;

        if (input.ResultadoAprovado && nivelNovo!.Value <= nivelAnterior)
        {
            return new GraduacaoRegistroResultado
            {
                Erro = "O nivel novo precisa ser maior que o nivel anterior para um resultado aprovado."
            };
        }

        var graduacao = new Graduacao
        {
            AlunoId = input.AlunoId,
            ExameGraduacaoId = exameGraduacaoId,
            ExameGraduacao = exameNovo,
            DataResultado = input.DataResultado,
            ResultadoAprovado = input.ResultadoAprovado,
            NivelAnterior = nivelAnterior,
            NivelNovo = nivelNovo,
            CertificadoEmitido = input.CertificadoEmitido,
            OmamoriAtualizado = input.OmamoriAtualizado,
            Observacoes = LimparOpcional(input.Observacoes)
        };

        await dbContext.Graduacoes.AddAsync(graduacao, cancellationToken);

        dbContext.HistoricosAlunos.Add(new HistoricoAluno
        {
            AlunoId = input.AlunoId,
            DataEvento = DateTime.Now,
            TipoEvento = "Graduacao",
            Descricao = MontarDescricaoHistorico(graduacao.NivelAnterior, graduacao.NivelNovo, graduacao.ResultadoAprovado)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new GraduacaoRegistroResultado
        {
            Sucesso = true,
            GraduacaoId = graduacao.Id,
            ExameGraduacaoId = graduacao.ExameGraduacaoId ?? exameNovo?.Id
        };
    }

    private async Task<NivelGraduacaoEnum> ObterNivelAtualAsync(int alunoId, CancellationToken cancellationToken)
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

    private static string MontarDescricaoHistorico(
        NivelGraduacaoEnum nivelAnterior,
        NivelGraduacaoEnum? nivelNovo,
        bool aprovado)
    {
        if (!aprovado)
        {
            return $"Resultado de graduacao registrado como reprovado. Nivel mantido em {nivelAnterior}.";
        }

        return $"Resultado de graduacao aprovado. Nivel atualizado de {nivelAnterior} para {nivelNovo}.";
    }

    private static string? LimparOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
