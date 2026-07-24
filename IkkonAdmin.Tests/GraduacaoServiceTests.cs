using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace IkkonAdmin.Tests;

public class GraduacaoServiceTests
{
    [Fact]
    public async Task CriarExameAsync_NormalizaDadosERetornaId()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.CriarExameAsync(new ExameGraduacao
        {
            DataExame = new DateOnly(2026, 7, 10),
            Local = " Dojo principal ",
            NivelPretendido = NivelGraduacaoEnum.Basico,
            Observacoes = " Trazer certificado "
        });

        var exameId = Assert.IsType<int>(result.Value);
        var exame = await dbContext.ExamesGraduacao.FindAsync(exameId);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Exame de graduação criado com sucesso.", result.Message);
        Assert.Equal("Dojo principal", exame?.Local);
        Assert.Equal("Trazer certificado", exame?.Observacoes);
    }

    [Fact]
    public async Task RegistrarResultadoAsync_RegistraAprovacaoComExameExistenteEHistorico()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Marina Tanaka", StatusAlunoEnum.Ativo);
        var exame = CriarExame();

        dbContext.AddRange(aluno, exame);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.RegistrarResultadoAsync(new GraduacaoRegistroInput
        {
            AlunoId = aluno.Id,
            ExameGraduacaoId = exame.Id,
            DataResultado = new DateOnly(2026, 7, 12),
            ResultadoAprovado = true,
            NivelNovo = NivelGraduacaoEnum.Basico,
            CertificadoEmitido = true,
            OmamoriAtualizado = true,
            Observacoes = " Aprovada com bom desempenho "
        });

        var payload = Assert.IsType<GraduacaoRegistroResultado>(result.Value);
        var graduacao = await dbContext.Graduacoes.FindAsync(payload.GraduacaoId);
        var historico = await dbContext.HistoricosAlunos.SingleAsync(x => x.AlunoId == aluno.Id);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal("Resultado de graduação registrado com sucesso.", result.Message);
        Assert.Equal(exame.Id, payload.ExameGraduacaoId);
        Assert.Equal(aluno.Id, graduacao?.AlunoId);
        Assert.Equal(exame.Id, graduacao?.ExameGraduacaoId);
        Assert.True(graduacao?.ResultadoAprovado);
        Assert.Equal(NivelGraduacaoEnum.Iniciante, graduacao?.NivelAnterior);
        Assert.Equal(NivelGraduacaoEnum.Basico, graduacao?.NivelNovo);
        Assert.True(graduacao?.CertificadoEmitido);
        Assert.True(graduacao?.OmamoriAtualizado);
        Assert.Equal("Aprovada com bom desempenho", graduacao?.Observacoes);
        Assert.Equal(TestClock.FixedNow, historico.DataEvento);
        Assert.Equal("Graduacao", historico.TipoEvento);
    }

    [Fact]
    public async Task RegistrarResultadoAsync_CriaExameQuandoDataNovaInformada()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Kenji Mori", StatusAlunoEnum.Ativo);

        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.RegistrarResultadoAsync(new GraduacaoRegistroInput
        {
            AlunoId = aluno.Id,
            DataExameNovo = new DateOnly(2026, 8, 10),
            LocalExameNovo = " Dojo secundário ",
            NivelPretendidoExameNovo = NivelGraduacaoEnum.Basico,
            DataResultado = new DateOnly(2026, 8, 11),
            ResultadoAprovado = false,
            Observacoes = " Reavaliar postura "
        });

        var payload = Assert.IsType<GraduacaoRegistroResultado>(result.Value);
        var exame = await dbContext.ExamesGraduacao.FindAsync(payload.ExameGraduacaoId);
        var graduacao = await dbContext.Graduacoes.FindAsync(payload.GraduacaoId);

        Assert.True(result.Success);
        Assert.NotNull(exame);
        Assert.Equal(new DateOnly(2026, 8, 10), exame.DataExame);
        Assert.Equal("Dojo secundário", exame.Local);
        Assert.Equal(NivelGraduacaoEnum.Basico, exame.NivelPretendido);
        Assert.False(graduacao?.ResultadoAprovado);
        Assert.Null(graduacao?.NivelNovo);
        Assert.Equal("Reavaliar postura", graduacao?.Observacoes);
    }

    [Fact]
    public async Task RegistrarResultadoAsync_RetornaNaoEncontradoQuandoAlunoNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.RegistrarResultadoAsync(CriarInputValido(alunoId: 123));

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Equal("Aluno não encontrado.", result.Message);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task RegistrarResultadoAsync_RetornaErroQuandoAlunoNaoEstaAtivo()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Bruno Dias", StatusAlunoEnum.Inativo);

        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.RegistrarResultadoAsync(CriarInputValido(aluno.Id));

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(GraduacaoRegistroInput.AlunoId), erro.Field);
        Assert.Equal("Somente alunos ativos podem receber registro de graduação.", erro.Message);
    }

    [Fact]
    public async Task RegistrarResultadoAsync_RetornaErroQuandoAprovadoSemNivelNovo()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Ana Mori", StatusAlunoEnum.Ativo);

        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);
        var input = CriarInputValido(aluno.Id);
        input.NivelNovo = null;

        var result = await service.RegistrarResultadoAsync(input);

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(GraduacaoRegistroInput.NivelNovo), erro.Field);
        Assert.Equal("Informe o nível novo para registrar resultado aprovado.", erro.Message);
    }

    [Fact]
    public async Task RegistrarResultadoAsync_RetornaErroQuandoNaoHaExameNemDataNova()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Ana Mori", StatusAlunoEnum.Ativo);

        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);
        var input = CriarInputValido(aluno.Id);
        input.DataExameNovo = null;
        input.ExameGraduacaoId = null;

        var result = await service.RegistrarResultadoAsync(input);

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(GraduacaoRegistroInput.ExameGraduacaoId), erro.Field);
        Assert.Equal("Selecione um exame existente ou informe a data para criar um novo exame.", erro.Message);
    }

    [Fact]
    public async Task RegistrarResultadoAsync_RetornaNaoEncontradoQuandoExameNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Ana Mori", StatusAlunoEnum.Ativo);

        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);
        var input = CriarInputValido(aluno.Id);
        input.ExameGraduacaoId = 123;
        input.DataExameNovo = null;

        var result = await service.RegistrarResultadoAsync(input);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Equal("Exame informado não encontrado.", result.Message);
        Assert.Null(result.Value);
    }

    [Fact]
    public async Task RegistrarResultadoAsync_RetornaErroQuandoNivelNovoNaoAvanca()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Rafael Sato", StatusAlunoEnum.Ativo);

        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        dbContext.Graduacoes.Add(new Graduacao
        {
            AlunoId = aluno.Id,
            DataResultado = new DateOnly(2026, 6, 10),
            ResultadoAprovado = true,
            NivelAnterior = NivelGraduacaoEnum.Iniciante,
            NivelNovo = NivelGraduacaoEnum.Basico
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);
        var input = CriarInputValido(aluno.Id);
        input.NivelNovo = NivelGraduacaoEnum.Basico;

        var result = await service.RegistrarResultadoAsync(input);

        var erro = Assert.Single(result.Errors);
        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Equal(nameof(GraduacaoRegistroInput.NivelNovo), erro.Field);
        Assert.Equal("O nível novo precisa ser maior que o nível anterior para um resultado aprovado.", erro.Message);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static GraduacaoService CriarService(ApplicationDbContext dbContext)
    {
        return new GraduacaoService(dbContext, new TestClock(), new GraduacaoQueryService(dbContext));
    }

    private static GraduacaoRegistroInput CriarInputValido(int alunoId)
    {
        return new GraduacaoRegistroInput
        {
            AlunoId = alunoId,
            DataExameNovo = new DateOnly(2026, 7, 10),
            NivelPretendidoExameNovo = NivelGraduacaoEnum.Basico,
            DataResultado = new DateOnly(2026, 7, 12),
            ResultadoAprovado = true,
            NivelNovo = NivelGraduacaoEnum.Basico
        };
    }

    private static Aluno CriarAluno(string nome, StatusAlunoEnum status)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = Guid.NewGuid().ToString("N")[..11],
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = status
        };
    }

    private static ExameGraduacao CriarExame()
    {
        return new ExameGraduacao
        {
            DataExame = new DateOnly(2026, 7, 10),
            Local = "Dojo",
            NivelPretendido = NivelGraduacaoEnum.Basico
        };
    }

    private sealed class TestClock : IClock
    {
        private static readonly DateTime FixedUtcNow = new(2026, 7, 13, 15, 0, 0, DateTimeKind.Utc);
        public static readonly DateTime FixedNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Local);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedNow;
        public DateTime Today => FixedNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
