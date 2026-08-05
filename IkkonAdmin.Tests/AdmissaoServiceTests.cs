using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IkkonAdmin.Tests;

public class AdmissaoServiceTests
{
    [Fact]
    public async Task CriarAsync_NormalizaDadosERetornaId()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.CriarAsync(new Admissao
        {
            NomeInteressado = " Marina Tanaka ",
            DataAulaExperimental = new DateOnly(2026, 7, 10),
            Status = StatusAdmissaoEnum.EmAndamento,
            ChecklistObservacoes = " Primeira aula confirmada "
        });

        var admissaoId = Assert.IsType<int>(result.Value);
        var admissao = await dbContext.Admissoes.FindAsync(admissaoId);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.NotNull(admissao);
        Assert.Equal("Marina Tanaka", admissao.NomeInteressado);
        Assert.Equal("Primeira aula confirmada", admissao.ChecklistObservacoes);
        Assert.Equal("Admissão registrada com sucesso.", result.Message);
    }

    [Fact]
    public async Task CriarAsync_BloqueiaStatusMatriculado()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.CriarAsync(new Admissao
        {
            NomeInteressado = "Marina Tanaka",
            DataAulaExperimental = new DateOnly(2026, 7, 10),
            Status = StatusAdmissaoEnum.Matriculado
        });

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(Admissao.Status), erro.Field);
        Assert.Equal("Use o status Matriculado somente após criar a matrícula.", erro.Message);
        Assert.Empty(dbContext.Admissoes);
    }

    [Fact]
    public async Task CriarMatriculaAsync_CriaAlunoVinculoHistoricoEAtualizaAdmissao()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Base", Modalidade = "Taiko", Ativa = true };
        var admissao = new Admissao
        {
            NomeInteressado = "Marina Tanaka",
            DataAulaExperimental = new DateOnly(2026, 7, 10),
            Status = StatusAdmissaoEnum.EmAndamento
        };

        dbContext.AddRange(turma, admissao);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.CriarMatriculaAsync(
            admissao.Id,
            new AdmissaoMatriculaInput
            {
                CPF = "123.456.789-01",
                RG = " 12.345.678-9 ",
                Celular = " (11) 99999-0001 ",
                Email = " marina@example.com ",
                TurmaId = turma.Id,
                ObservacoesAluno = " Nova aluna "
            });
        var matricula = Assert.IsType<AdmissaoMatriculaResultado>(result.Value);

        var aluno = await dbContext.Alunos
            .Include(x => x.AlunoTurmas)
            .FirstOrDefaultAsync(x => x.Id == matricula.AlunoId);
        var admissaoAtualizada = await dbContext.Admissoes.FindAsync(admissao.Id);
        var historico = await dbContext.HistoricosAlunos.SingleAsync(x => x.AlunoId == matricula.AlunoId);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Matrícula criada com sucesso e vinculada à admissão.", result.Message);
        Assert.NotNull(aluno);
        Assert.Equal("Marina Tanaka", aluno.NomeCompleto);
        Assert.Equal("12345678901", aluno.CPF);
        Assert.Equal(new DateOnly(2026, 7, 13), aluno.DataEntrada);
        Assert.Equal(turma.Id, aluno.TurmaId);
        Assert.Equal(StatusAlunoEnum.Ativo, aluno.Status);
        Assert.Equal("Nova aluna", aluno.Observacoes);
        Assert.Contains(aluno.AlunoTurmas, x =>
            x.TurmaId == turma.Id &&
            x.DataVinculo == TestClock.FixedUtcNow);
        Assert.Equal(matricula.AlunoId, admissaoAtualizada?.AlunoId);
        Assert.Equal(new DateOnly(2026, 7, 13), admissaoAtualizada?.DataMatricula);
        Assert.Equal(StatusAdmissaoEnum.Matriculado, admissaoAtualizada?.Status);
        Assert.Equal(TestClock.FixedNow, historico.DataEvento);
        Assert.Equal("Admissao", historico.TipoEvento);
    }

    [Fact]
    public async Task CriarMatriculaAsync_BloqueiaCpfDuplicado()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Alunos.Add(new Aluno
        {
            NomeCompleto = "Aluno existente",
            CPF = "123.456.789-01",
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo
        });
        dbContext.Admissoes.Add(new Admissao
        {
            NomeInteressado = "Novo interessado",
            DataAulaExperimental = new DateOnly(2026, 7, 10),
            Status = StatusAdmissaoEnum.EmAndamento
        });
        await dbContext.SaveChangesAsync();

        var admissao = await dbContext.Admissoes.SingleAsync();
        var service = CriarService(dbContext);

        var result = await service.CriarMatriculaAsync(
            admissao.Id,
            new AdmissaoMatriculaInput { CPF = "12345678901" });

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(AdmissaoMatriculaInput.CPF), erro.Field);
        Assert.Equal("Já existe um aluno cadastrado com esse CPF.", erro.Message);
        Assert.Single(dbContext.Alunos);
    }

    [Fact]
    public async Task CriarMatriculaAsync_RetornaErroQuandoCpfInvalido()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Admissoes.Add(new Admissao
        {
            NomeInteressado = "Novo interessado",
            DataAulaExperimental = new DateOnly(2026, 7, 10),
            Status = StatusAdmissaoEnum.EmAndamento
        });
        await dbContext.SaveChangesAsync();

        var admissao = await dbContext.Admissoes.SingleAsync();
        var service = CriarService(dbContext);

        var result = await service.CriarMatriculaAsync(
            admissao.Id,
            new AdmissaoMatriculaInput { CPF = "123" });

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(AdmissaoMatriculaInput.CPF), erro.Field);
        Assert.Equal("Informe um CPF válido com 11 dígitos.", erro.Message);
        Assert.Empty(dbContext.Alunos);
    }

    [Fact]
    public async Task CriarMatriculaAsync_RetornaNaoEncontradoQuandoAdmissaoNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.CriarMatriculaAsync(
            123,
            new AdmissaoMatriculaInput { CPF = "12345678901" });

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Equal("Admissão não encontrada.", result.Message);
        Assert.Null(result.Value);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task CriarMatriculaAsync_RetornaErroQuandoAdmissaoJaPossuiAluno()
    {
        await using var dbContext = CriarDbContext();
        var aluno = new Aluno
        {
            NomeCompleto = "Aluno existente",
            CPF = "12345678901",
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo
        };
        var admissao = new Admissao
        {
            NomeInteressado = "Novo interessado",
            DataAulaExperimental = new DateOnly(2026, 7, 10),
            Status = StatusAdmissaoEnum.Matriculado,
            Aluno = aluno
        };

        dbContext.AddRange(aluno, admissao);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.CriarMatriculaAsync(
            admissao.Id,
            new AdmissaoMatriculaInput { CPF = "55566677788" });

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal("Esta admissão já possui matrícula vinculada.", result.Message);
        Assert.Equal(1, await dbContext.Alunos.CountAsync());
    }

    [Fact]
    public async Task AtualizarProcessoAsync_AtualizaCamposDoProcesso()
    {
        await using var dbContext = CriarDbContext();
        var admissao = new Admissao
        {
            NomeInteressado = "Interessado",
            DataAulaExperimental = new DateOnly(2026, 7, 10),
            Status = StatusAdmissaoEnum.AulaExperimentalAgendada
        };

        dbContext.Admissoes.Add(admissao);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.AtualizarProcessoAsync(
            admissao.Id,
            StatusAdmissaoEnum.EmAndamento,
            contratoAssinado: true,
            pagamentoInicialConfirmado: true,
            integracaoConcluida: false,
            checklistObservacoes: " Contrato revisado ");

        var admissaoAtualizada = await dbContext.Admissoes.FindAsync(admissao.Id);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Processo de admissão atualizado.", result.Message);
        Assert.Equal(StatusAdmissaoEnum.EmAndamento, admissaoAtualizada?.Status);
        Assert.True(admissaoAtualizada?.ContratoAssinado);
        Assert.True(admissaoAtualizada?.PagamentoInicialConfirmado);
        Assert.False(admissaoAtualizada?.IntegracaoConcluida);
        Assert.Equal("Contrato revisado", admissaoAtualizada?.ChecklistObservacoes);
    }

    [Fact]
    public async Task AtualizarProcessoAsync_ImpedeMatriculadoSemAluno()
    {
        await using var dbContext = CriarDbContext();
        var admissao = new Admissao
        {
            NomeInteressado = "Interessado",
            DataAulaExperimental = new DateOnly(2026, 7, 10),
            Status = StatusAdmissaoEnum.EmAndamento
        };

        dbContext.Admissoes.Add(admissao);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var atualizado = await service.AtualizarProcessoAsync(
            admissao.Id,
            StatusAdmissaoEnum.Matriculado,
            contratoAssinado: true,
            pagamentoInicialConfirmado: true,
            integracaoConcluida: true,
            checklistObservacoes: "ok");

        var admissaoAtualizada = await dbContext.Admissoes.FindAsync(admissao.Id);
        var erro = Assert.Single(atualizado.Errors);

        Assert.False(atualizado.Success);
        Assert.Equal(OperationResultStatus.ValidationError, atualizado.Status);
        Assert.Equal(nameof(Admissao.Status), erro.Field);
        Assert.Equal("Crie a matrícula antes de definir o status Matriculado.", erro.Message);
        Assert.Equal(StatusAdmissaoEnum.EmAndamento, admissaoAtualizada?.Status);
    }

    [Fact]
    public async Task AtualizarProcessoAsync_RetornaNaoEncontradoQuandoAdmissaoNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AtualizarProcessoAsync(
            123,
            StatusAdmissaoEnum.EmAndamento,
            contratoAssinado: true,
            pagamentoInicialConfirmado: true,
            integracaoConcluida: true,
            checklistObservacoes: null);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
        Assert.Equal("Admissão não encontrada.", result.Message);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AdmissaoService CriarService(ApplicationDbContext dbContext)
    {
        return new AdmissaoService(
            dbContext,
            new TestClock(),
            new AlunoQueryService(dbContext));
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
