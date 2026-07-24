using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class AlunoServiceTests
{
    [Fact]
    public async Task CriarAsync_NormalizaDadosECriaVinculoComTurmaPrincipal()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Base", Modalidade = "Taiko", Ativa = true };

        dbContext.Turmas.Add(turma);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.CriarAsync(new Aluno
        {
            NomeCompleto = " Ana Mori ",
            CPF = "123.456.789-01",
            RG = " 12.345.678-9 ",
            DataEntrada = new DateOnly(2026, 7, 1),
            TurmaId = turma.Id,
            Status = StatusAlunoEnum.Ativo,
            Observacoes = " Nova aluna "
        });

        var alunoId = Assert.IsType<int>(result.Value);
        var aluno = await dbContext.Alunos
            .Include(x => x.AlunoTurmas)
            .FirstAsync(x => x.Id == alunoId);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Aluno cadastrado com sucesso.", result.Message);
        Assert.Equal("Ana Mori", aluno.NomeCompleto);
        Assert.Equal("12345678901", aluno.CPF);
        Assert.Equal("12.345.678-9", aluno.RG);
        Assert.Equal("Nova aluna", aluno.Observacoes);
        Assert.Contains(aluno.AlunoTurmas, x =>
            x.TurmaId == turma.Id &&
            x.DataVinculo == TestClock.FixedUtcNow);
    }

    [Fact]
    public async Task CriarAsync_RetornaErroQuandoCpfInvalido()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.CriarAsync(new Aluno
        {
            NomeCompleto = "Ana Mori",
            CPF = "123",
            DataEntrada = new DateOnly(2026, 7, 1),
            Status = StatusAlunoEnum.Ativo
        });

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(Aluno.CPF), erro.Field);
        Assert.Equal("Informe um CPF válido com 11 dígitos.", erro.Message);
        Assert.Empty(dbContext.Alunos);
    }

    [Fact]
    public async Task CriarAsync_RetornaErroQuandoCpfDuplicado()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Alunos.Add(CriarAluno("Aluno existente", "123.456.789-01"));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.CriarAsync(new Aluno
        {
            NomeCompleto = "Novo aluno",
            CPF = "12345678901",
            DataEntrada = new DateOnly(2026, 7, 1),
            Status = StatusAlunoEnum.Ativo
        });

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(Aluno.CPF), erro.Field);
        Assert.Equal("Já existe um aluno cadastrado com este CPF.", erro.Message);
        Assert.Equal(1, await dbContext.Alunos.CountAsync());
    }

    [Fact]
    public async Task AtualizarAsync_NormalizaDadosEGaranteVinculoComTurmaPrincipal()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Intermediaria", Modalidade = "Taiko", Ativa = true };
        var aluno = CriarAluno("Kenji Mori", "12345678901");

        dbContext.AddRange(turma, aluno);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.AtualizarAsync(
            aluno.Id,
            new Aluno
            {
                NomeCompleto = " Kenji Mori Atualizado ",
                CPF = "123.456.789-01",
                RG = " 98.765.432-1 ",
                DataEntrada = new DateOnly(2026, 7, 2),
                TurmaId = turma.Id,
                Status = StatusAlunoEnum.Inativo,
                Observacoes = " Reposicionamento de turma "
            });

        var atualizado = await dbContext.Alunos
            .Include(x => x.AlunoTurmas)
            .FirstAsync(x => x.Id == aluno.Id);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Aluno atualizado com sucesso.", result.Message);
        Assert.Equal("Kenji Mori Atualizado", atualizado.NomeCompleto);
        Assert.Equal("12345678901", atualizado.CPF);
        Assert.Equal("98.765.432-1", atualizado.RG);
        Assert.Equal(StatusAlunoEnum.Inativo, atualizado.Status);
        Assert.Equal("Reposicionamento de turma", atualizado.Observacoes);
        Assert.Contains(atualizado.AlunoTurmas, x =>
            x.TurmaId == turma.Id &&
            x.DataVinculo == TestClock.FixedUtcNow);
    }

    [Fact]
    public async Task AtualizarAsync_RetornaErroQuandoCpfDuplicado()
    {
        await using var dbContext = CriarDbContext();
        var alunoExistente = CriarAluno("Aluno existente", "11122233344");
        var alunoAtual = CriarAluno("Aluno atual", "55566677788");

        dbContext.AddRange(alunoExistente, alunoAtual);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.AtualizarAsync(
            alunoAtual.Id,
            new Aluno
            {
                NomeCompleto = "Aluno atual",
                CPF = "111.222.333-44",
                DataEntrada = new DateOnly(2026, 7, 1),
                Status = StatusAlunoEnum.Ativo
            });

        var erro = Assert.Single(result.Errors);
        var semAlteracao = await dbContext.Alunos.FindAsync(alunoAtual.Id);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(Aluno.CPF), erro.Field);
        Assert.Equal("Já existe um aluno cadastrado com este CPF.", erro.Message);
        Assert.Equal("55566677788", semAlteracao?.CPF);
    }

    [Fact]
    public async Task AtualizarAsync_RetornaNaoEncontradoQuandoAlunoNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AtualizarAsync(
            123,
            new Aluno
            {
                NomeCompleto = "Aluno",
                CPF = "12345678901",
                DataEntrada = new DateOnly(2026, 7, 1),
                Status = StatusAlunoEnum.Ativo
            });

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("Aluno não encontrado.", result.Message);
    }

    [Fact]
    public async Task AlterarStatusAsync_AtualizaStatus()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Ana Mori", "12345678901");

        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.AlterarStatusAsync(aluno.Id, StatusAlunoEnum.Desligado);
        var atualizado = await dbContext.Alunos.FindAsync(aluno.Id);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Status do aluno atualizado.", result.Message);
        Assert.Equal(StatusAlunoEnum.Desligado, atualizado?.Status);
    }

    [Fact]
    public async Task AlterarStatusAsync_RetornaNaoEncontradoQuandoAlunoNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AlterarStatusAsync(123, StatusAlunoEnum.Desligado);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("Aluno não encontrado.", result.Message);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AlunoService CriarService(ApplicationDbContext dbContext)
    {
        return new AlunoService(dbContext, new TestClock(), new AlunoQueryService(dbContext));
    }

    private static Aluno CriarAluno(string nome, string cpf)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = cpf,
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo
        };
    }

    private sealed class TestClock : IClock
    {
        public static readonly DateTime FixedUtcNow = new(2026, 7, 13, 15, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime FixedNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Local);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedNow;
        public DateTime Today => FixedNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
