using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class ConfiguracaoQueryServiceTests
{
    [Fact]
    public async Task ObterFormularioAsync_CriaConfiguracaoPadraoQuandoNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var form = await service.ObterFormularioAsync();
        var config = await dbContext.ConfiguracoesSistema.SingleAsync();

        Assert.Equal("Escola de Taiko Ikkon", form.NomeEscola);
        Assert.Equal(260m, form.ValorMensalidadePadrao);
        Assert.Equal(TestClock.FixedUtcNow, form.UltimaAtualizacaoUtc);
        Assert.Equal(TestClock.FixedUtcNow, config.UltimaAtualizacaoUtc);
    }

    [Fact]
    public async Task ObterPainelAsync_CalculaResumoComJanelaDeTrintaDias()
    {
        await using var dbContext = CriarDbContext();
        var turmaAtiva = new Turma { Nome = "Taiko Base", Modalidade = "Taiko", Ativa = true };
        var turmaInativa = new Turma { Nome = "Turma antiga", Modalidade = "Taiko", Ativa = false };
        var alunoAtivo = CriarAluno("Ana Mori", StatusAlunoEnum.Ativo);
        var alunoInativo = CriarAluno("Bruno Dias", StatusAlunoEnum.Inativo);

        dbContext.AddRange(turmaAtiva, turmaInativa, alunoAtivo, alunoInativo);
        await dbContext.SaveChangesAsync();

        dbContext.Mensalidades.AddRange(
            CriarMensalidade(alunoAtivo.Id, StatusMensalidadeEnum.Atrasado),
            CriarMensalidade(alunoInativo.Id, StatusMensalidadeEnum.Pendente));
        dbContext.Desligamentos.AddRange(
            CriarDesligamento(alunoAtivo.Id, confirmacao: null),
            CriarDesligamento(alunoInativo.Id, confirmacao: new DateOnly(2026, 7, 12)));
        dbContext.ExamesGraduacao.AddRange(
            new ExameGraduacao { DataExame = new DateOnly(2026, 7, 20), NivelPretendido = NivelGraduacaoEnum.Basico },
            new ExameGraduacao { DataExame = new DateOnly(2026, 8, 12), NivelPretendido = NivelGraduacaoEnum.Basico },
            new ExameGraduacao { DataExame = new DateOnly(2026, 8, 20), NivelPretendido = NivelGraduacaoEnum.Basico });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var painel = await service.ObterPainelAsync();

        Assert.Equal(1, painel.Resumo.AlunosAtivos);
        Assert.Equal(1, painel.Resumo.TurmasAtivas);
        Assert.Equal(1, painel.Resumo.MensalidadesAtrasadas);
        Assert.Equal(1, painel.Resumo.DesligamentosEmAberto);
        Assert.Equal(2, painel.Resumo.ExamesProximos30Dias);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ConfiguracaoQueryService CriarService(ApplicationDbContext dbContext)
    {
        var clock = new TestClock();
        return new ConfiguracaoQueryService(
            dbContext,
            clock,
            new ConfiguracaoSistemaProvider(dbContext, clock));
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

    private static Mensalidade CriarMensalidade(int alunoId, StatusMensalidadeEnum status)
    {
        return new Mensalidade
        {
            AlunoId = alunoId,
            Competencia = new DateOnly(2026, 7, 1),
            DataVencimento = new DateOnly(2026, 7, 10),
            ValorBase = 260m,
            ValorFinal = 260m,
            Status = status
        };
    }

    private static Desligamento CriarDesligamento(int alunoId, DateOnly? confirmacao)
    {
        return new Desligamento
        {
            AlunoId = alunoId,
            DataSolicitacao = new DateOnly(2026, 7, 10),
            DataConfirmacao = confirmacao,
            Motivo = "Solicitado"
        };
    }

    private sealed class TestClock : IClock
    {
        public static readonly DateTime FixedUtcNow = new(2026, 7, 13, 15, 0, 0, DateTimeKind.Utc);
        private static readonly DateTime FixedNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Local);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedNow;
        public DateTime Today => FixedNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
