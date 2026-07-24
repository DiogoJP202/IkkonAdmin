using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AdmissaoService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAdmissaoQueryService queryService,
    IAlunoQueryService alunoQueryService) : IAdmissaoService
{
    public Task<IReadOnlyList<Admissao>> ListarAsync(
        string? busca = null,
        StatusAdmissaoEnum? status = null,
        CancellationToken cancellationToken = default)
    {
        return queryService.ListarAsync(busca, status, cancellationToken);
    }

    public Task<Admissao?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default)
    {
        return queryService.ObterDetalhesAsync(id, cancellationToken);
    }

    public Task<IReadOnlyList<Turma>> ListarTurmasAsync(CancellationToken cancellationToken = default)
    {
        return queryService.ListarTurmasAsync(cancellationToken);
    }

    public async Task<OperationResult<int>> CriarAsync(Admissao admissao, CancellationToken cancellationToken = default)
    {
        admissao.NomeInteressado = admissao.NomeInteressado.Trim();
        admissao.ChecklistObservacoes = LimparOpcional(admissao.ChecklistObservacoes);

        if (admissao.Status == StatusAdmissaoEnum.Matriculado)
        {
            return OperationResult<int>.Fail(
                "Use o status Matriculado somente após criar a matrícula.",
                nameof(Admissao.Status));
        }

        await dbContext.Admissoes.AddAsync(admissao, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult<int>.Ok(admissao.Id, "Admissão registrada com sucesso.");
    }

    public async Task<OperationResult> AtualizarProcessoAsync(
        int id,
        StatusAdmissaoEnum status,
        bool contratoAssinado,
        bool pagamentoInicialConfirmado,
        bool integracaoConcluida,
        string? checklistObservacoes,
        CancellationToken cancellationToken = default)
    {
        var admissao = await dbContext.Admissoes
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (admissao is null)
        {
            return OperationResult.NotFound("Admissão não encontrada.");
        }

        if (status == StatusAdmissaoEnum.Matriculado && !admissao.AlunoId.HasValue)
        {
            return OperationResult.Fail(
                "Crie a matrícula antes de definir o status Matriculado.",
                nameof(Admissao.Status));
        }

        admissao.Status = status;
        admissao.ContratoAssinado = contratoAssinado;
        admissao.PagamentoInicialConfirmado = pagamentoInicialConfirmado;
        admissao.IntegracaoConcluida = integracaoConcluida;
        admissao.ChecklistObservacoes = LimparOpcional(checklistObservacoes);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Processo de admissão atualizado.");
    }

    public async Task<OperationResult<AdmissaoMatriculaResultado>> CriarMatriculaAsync(
        int admissaoId,
        AdmissaoMatriculaInput input,
        CancellationToken cancellationToken = default)
    {
        var admissao = await dbContext.Admissoes
            .FirstOrDefaultAsync(x => x.Id == admissaoId, cancellationToken);

        if (admissao is null)
        {
            return OperationResult<AdmissaoMatriculaResultado>.NotFound("Admissão não encontrada.");
        }

        if (admissao.AlunoId.HasValue)
        {
            return OperationResult<AdmissaoMatriculaResultado>.Fail("Esta admissão já possui matrícula vinculada.");
        }

        var cpf = SomenteDigitos(input.CPF);
        if (cpf.Length != 11)
        {
            return OperationResult<AdmissaoMatriculaResultado>.Fail(
                "Informe um CPF válido com 11 dígitos.",
                nameof(AdmissaoMatriculaInput.CPF));
        }

        if (await alunoQueryService.ExisteCpfAsync(cpf, cancellationToken: cancellationToken))
        {
            return OperationResult<AdmissaoMatriculaResultado>.Fail(
                "Já existe um aluno cadastrado com esse CPF.",
                nameof(AdmissaoMatriculaInput.CPF));
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var novoAluno = new Aluno
        {
            NomeCompleto = admissao.NomeInteressado,
            CPF = cpf,
            RG = LimparOpcional(input.RG),
            DataNascimento = input.DataNascimento,
            Endereco = LimparOpcional(input.Endereco),
            Celular = LimparOpcional(input.Celular),
            Email = LimparOpcional(input.Email),
            ContatoEmergencia = LimparOpcional(input.ContatoEmergencia),
            DataEntrada = clock.TodayDate,
            TurmaId = input.TurmaId,
            Status = StatusAlunoEnum.Ativo,
            Observacoes = LimparOpcional(input.ObservacoesAluno)
        };

        await dbContext.Alunos.AddAsync(novoAluno, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (novoAluno.TurmaId.HasValue)
        {
            dbContext.AlunosTurmas.Add(new AlunoTurma
            {
                AlunoId = novoAluno.Id,
                TurmaId = novoAluno.TurmaId.Value,
                DataVinculo = clock.UtcNow
            });
        }

        admissao.AlunoId = novoAluno.Id;
        admissao.DataMatricula = clock.TodayDate;
        admissao.Status = StatusAdmissaoEnum.Matriculado;

        dbContext.HistoricosAlunos.Add(new HistoricoAluno
        {
            AlunoId = novoAluno.Id,
            DataEvento = clock.Now,
            TipoEvento = "Admissao",
            Descricao = $"Matricula criada a partir da admissao #{admissao.Id}."
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return OperationResult<AdmissaoMatriculaResultado>.Ok(
            new AdmissaoMatriculaResultado
            {
                AlunoId = novoAluno.Id
            },
            "Matrícula criada com sucesso e vinculada à admissão.");
    }

    private static string SomenteDigitos(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        return new string(valor.Where(char.IsDigit).ToArray());
    }

    private static string? LimparOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
