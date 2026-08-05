using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class GraduacaoService(
    ApplicationDbContext dbContext,
    IClock clock,
    IGraduacaoQueryService queryService) : IGraduacaoService
{
    public async Task<OperationResult<int>> CriarExameAsync(
        ExameGraduacao exame,
        CancellationToken cancellationToken = default)
    {
        exame.Local = LimparOpcional(exame.Local);
        exame.Observacoes = LimparOpcional(exame.Observacoes);

        await dbContext.ExamesGraduacao.AddAsync(exame, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(exame.Id, "Exame de graduação criado com sucesso.");
    }

    public async Task<OperationResult<GraduacaoRegistroResultado>> RegistrarResultadoAsync(
        GraduacaoRegistroInput input,
        CancellationToken cancellationToken = default)
    {
        var aluno = await dbContext.Alunos
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == input.AlunoId, cancellationToken);

        if (aluno is null)
        {
            return OperationResult<GraduacaoRegistroResultado>.NotFound("Aluno não encontrado.");
        }

        if (aluno.Status != StatusAlunoEnum.Ativo)
        {
            return OperationResult<GraduacaoRegistroResultado>.Fail(
                "Somente alunos ativos podem receber registro de graduação.",
                nameof(GraduacaoRegistroInput.AlunoId));
        }

        if (input.ResultadoAprovado && !input.NivelNovo.HasValue)
        {
            return OperationResult<GraduacaoRegistroResultado>.Fail(
                "Informe o nível novo para registrar resultado aprovado.",
                nameof(GraduacaoRegistroInput.NivelNovo));
        }

        if (!input.ExameGraduacaoId.HasValue && !input.DataExameNovo.HasValue)
        {
            return OperationResult<GraduacaoRegistroResultado>.Fail(
                "Selecione um exame existente ou informe a data para criar um novo exame.",
                nameof(GraduacaoRegistroInput.ExameGraduacaoId));
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
                return OperationResult<GraduacaoRegistroResultado>.NotFound("Exame informado não encontrado.");
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

        var nivelAnterior = await queryService.ObterNivelAtualAsync(input.AlunoId, cancellationToken);

        var nivelNovo = input.ResultadoAprovado
            ? input.NivelNovo
            : null;

        if (input.ResultadoAprovado && nivelNovo!.Value <= nivelAnterior)
        {
            return OperationResult<GraduacaoRegistroResultado>.Fail(
                "O nível novo precisa ser maior que o nível anterior para um resultado aprovado.",
                nameof(GraduacaoRegistroInput.NivelNovo));
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
            DataEvento = clock.Now,
            TipoEvento = "Graduacao",
            Descricao = MontarDescricaoHistorico(graduacao.NivelAnterior, graduacao.NivelNovo, graduacao.ResultadoAprovado)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return OperationResult<GraduacaoRegistroResultado>.Ok(
            new GraduacaoRegistroResultado
            {
                GraduacaoId = graduacao.Id,
                ExameGraduacaoId = graduacao.ExameGraduacaoId ?? exameNovo?.Id
            },
            "Resultado de graduação registrado com sucesso.");
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
