using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AlunoService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAlunoQueryService queryService) : IAlunoService
{
    public async Task<OperationResult<int>> CriarAsync(Aluno aluno, CancellationToken cancellationToken = default)
    {
        NormalizarAluno(aluno);

        var cpfValidation = await ValidarCpfAsync(aluno.CPF, null, cancellationToken);
        if (!cpfValidation.Success)
        {
            return OperationResult<int>.Fail(cpfValidation.Message, cpfValidation.Errors);
        }

        if (aluno.TurmaId.HasValue && !aluno.AlunoTurmas.Any(x => x.TurmaId == aluno.TurmaId.Value))
        {
            aluno.AlunoTurmas.Add(new AlunoTurma
            {
                TurmaId = aluno.TurmaId.Value,
                DataVinculo = clock.UtcNow
            });
        }

        await dbContext.Alunos.AddAsync(aluno, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult<int>.Ok(aluno.Id, "Aluno cadastrado com sucesso.");
    }

    public async Task<OperationResult> AtualizarAsync(
        int id,
        Aluno alunoAtualizado,
        CancellationToken cancellationToken = default)
    {
        var aluno = await dbContext.Alunos
            .Include(x => x.AlunoTurmas)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (aluno is null)
        {
            return OperationResult.NotFound("Aluno não encontrado.");
        }

        NormalizarAluno(alunoAtualizado);

        var cpfValidation = await ValidarCpfAsync(alunoAtualizado.CPF, id, cancellationToken);
        if (!cpfValidation.Success)
        {
            return cpfValidation;
        }

        aluno.NomeCompleto = alunoAtualizado.NomeCompleto;
        aluno.CPF = alunoAtualizado.CPF;
        aluno.RG = alunoAtualizado.RG;
        aluno.DataNascimento = alunoAtualizado.DataNascimento;
        aluno.Endereco = alunoAtualizado.Endereco;
        aluno.Celular = alunoAtualizado.Celular;
        aluno.Email = alunoAtualizado.Email;
        aluno.ContatoEmergencia = alunoAtualizado.ContatoEmergencia;
        aluno.DataEntrada = alunoAtualizado.DataEntrada;
        aluno.TurmaId = alunoAtualizado.TurmaId;
        aluno.Status = alunoAtualizado.Status;
        aluno.Observacoes = alunoAtualizado.Observacoes;

        GarantirVinculoTurmaPrincipal(aluno);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Aluno atualizado com sucesso.");
    }

    public async Task<OperationResult> AlterarStatusAsync(
        int id,
        StatusAlunoEnum status,
        CancellationToken cancellationToken = default)
    {
        var aluno = await dbContext.Alunos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (aluno is null)
        {
            return OperationResult.NotFound("Aluno não encontrado.");
        }

        aluno.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Status do aluno atualizado.");
    }

    private async Task<OperationResult> ValidarCpfAsync(
        string cpf,
        int? ignorarAlunoId,
        CancellationToken cancellationToken)
    {
        if (cpf.Length != 11)
        {
            return OperationResult.Fail("Informe um CPF válido com 11 dígitos.", nameof(Aluno.CPF));
        }

        if (await queryService.ExisteCpfAsync(cpf, ignorarAlunoId, cancellationToken))
        {
            return OperationResult.Fail("Já existe um aluno cadastrado com este CPF.", nameof(Aluno.CPF));
        }

        return OperationResult.Ok("CPF válido.");
    }

    private void GarantirVinculoTurmaPrincipal(Aluno aluno)
    {
        if (!aluno.TurmaId.HasValue || aluno.AlunoTurmas.Any(x => x.TurmaId == aluno.TurmaId.Value))
        {
            return;
        }

        dbContext.AlunosTurmas.Add(new AlunoTurma
        {
            AlunoId = aluno.Id,
            TurmaId = aluno.TurmaId.Value,
            DataVinculo = clock.UtcNow
        });
    }

    private static void NormalizarAluno(Aluno aluno)
    {
        aluno.NomeCompleto = aluno.NomeCompleto?.Trim() ?? string.Empty;
        aluno.CPF = SomenteDigitos(aluno.CPF);
        aluno.RG = LimparOpcional(aluno.RG);
        aluno.Endereco = LimparOpcional(aluno.Endereco);
        aluno.Celular = LimparOpcional(aluno.Celular);
        aluno.Email = LimparOpcional(aluno.Email);
        aluno.ContatoEmergencia = LimparOpcional(aluno.ContatoEmergencia);
        aluno.Observacoes = LimparOpcional(aluno.Observacoes);
    }

    private static string SomenteDigitos(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        return new string(valor.Where(char.IsDigit).ToArray());
    }

    private static string? LimparOpcional(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}
