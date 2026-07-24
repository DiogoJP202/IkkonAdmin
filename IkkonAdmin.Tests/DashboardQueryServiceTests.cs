using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class DashboardQueryServiceTests
{
    [Fact]
    public async Task ObterDashboardAsync_FiltraPorTurmaPrincipalEVinculoSecundario()
    {
        await using var dbContext = CriarDbContext();
        var turmaBase = new Turma { Nome = "Shinobue Base", Modalidade = "Taiko", Ativa = true };
        var turmaOutra = new Turma { Nome = "Taiko Intermediaria", Modalidade = "Taiko", Ativa = true };

        var alunoPrincipal = CriarAluno("Kenji Mori", "111.111.111-11", turmaBase);
        var alunoVinculado = CriarAluno("Marina Tanaka", "222.222.222-22", turmaOutra);
        var alunoForaFiltro = CriarAluno("Rafael Sato", "333.333.333-33", turmaOutra);

        dbContext.AddRange(turmaBase, turmaOutra, alunoPrincipal, alunoVinculado, alunoForaFiltro);
        await dbContext.SaveChangesAsync();

        dbContext.AlunosTurmas.Add(new AlunoTurma
        {
            AlunoId = alunoVinculado.Id,
            TurmaId = turmaBase.Id,
            DataVinculo = new DateTime(2026, 7, 1)
        });

        var mensalidadePrincipal = CriarMensalidade(
            alunoPrincipal.Id,
            StatusMensalidadeEnum.Pendente,
            260m,
            260m,
            new DateOnly(2026, 7, 20));
        var mensalidadeVinculada = CriarMensalidade(
            alunoVinculado.Id,
            StatusMensalidadeEnum.Pendente,
            260m,
            210m,
            new DateOnly(2026, 7, 10));
        var mensalidadeForaFiltro = CriarMensalidade(
            alunoForaFiltro.Id,
            StatusMensalidadeEnum.Pago,
            260m,
            260m,
            new DateOnly(2026, 7, 10));

        dbContext.Mensalidades.AddRange(mensalidadePrincipal, mensalidadeVinculada, mensalidadeForaFiltro);
        await dbContext.SaveChangesAsync();

        dbContext.Pagamentos.AddRange(
            new Pagamento
            {
                AlunoId = alunoPrincipal.Id,
                MensalidadeId = mensalidadePrincipal.Id,
                DataPagamento = new DateTime(2026, 7, 5, 12, 0, 0),
                ValorPago = 100m
            },
            new Pagamento
            {
                AlunoId = alunoForaFiltro.Id,
                MensalidadeId = mensalidadeForaFiltro.Id,
                DataPagamento = new DateTime(2026, 7, 5, 12, 0, 0),
                ValorPago = 999m
            });

        dbContext.HistoricosAlunos.AddRange(
            CriarHistorico(alunoPrincipal.Id, "Financeiro", "Pagamento parcial"),
            CriarHistorico(alunoVinculado.Id, "Graduacao", "Apto para exame"),
            CriarHistorico(alunoForaFiltro.Id, "Admissao", "Fora do filtro"));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var dashboard = await service.ObterDashboardAsync(2026, 7, turmaBase.Id);

        Assert.Equal(turmaBase.Id, dashboard.TurmaIdFiltro);
        Assert.Equal(2, dashboard.QuantidadeAlunosAtivos);
        Assert.Equal(2, dashboard.MensalidadesPendentes);
        Assert.Equal(1, dashboard.MensalidadesAtrasadas);
        Assert.Equal(210m, dashboard.TotalEmAtraso);
        Assert.Equal(1, dashboard.QuantidadeAlunosInadimplentes);
        Assert.Equal(100m, dashboard.ReceitaRecebidaNoMes);
        Assert.Equal(2, dashboard.ProximosVencimentos.Count);
        Assert.Equal(2, dashboard.AtividadesRecentes.Count);
        Assert.DoesNotContain(dashboard.AtividadesRecentes, x => x.Descricao.Contains("Fora do filtro"));
    }

    [Fact]
    public async Task ObterDashboardAsync_NormalizaReferenciaETurmaInvalida()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Turmas.Add(new Turma { Nome = "Turma unica", Modalidade = "Taiko", Ativa = true });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var dashboard = await service.ObterDashboardAsync(1900, 99, turmaId: 999);

        Assert.Equal(2020, dashboard.AnoReferencia);
        Assert.Equal(12, dashboard.MesReferencia);
        Assert.Equal("Dezembro/2020", dashboard.MesAnoReferenciaDescricao);
        Assert.Null(dashboard.TurmaIdFiltro);
        Assert.Single(dashboard.TurmasDisponiveis);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static DashboardQueryService CriarService(ApplicationDbContext dbContext)
    {
        return new DashboardQueryService(dbContext, new TestClock());
    }

    private static Aluno CriarAluno(string nome, string cpf, Turma turma)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = cpf,
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = StatusAlunoEnum.Ativo,
            Turma = turma
        };
    }

    private static Mensalidade CriarMensalidade(
        int alunoId,
        StatusMensalidadeEnum status,
        decimal valorBase,
        decimal valorFinal,
        DateOnly vencimento)
    {
        return new Mensalidade
        {
            AlunoId = alunoId,
            Competencia = new DateOnly(vencimento.Year, vencimento.Month, 1),
            DataVencimento = vencimento,
            ValorBase = valorBase,
            ValorFinal = valorFinal,
            Status = status,
            DataPagamento = status == StatusMensalidadeEnum.Pago ? vencimento : null
        };
    }

    private static HistoricoAluno CriarHistorico(int alunoId, string tipo, string descricao)
    {
        return new HistoricoAluno
        {
            AlunoId = alunoId,
            TipoEvento = tipo,
            Descricao = descricao,
            DataEvento = new DateTime(2026, 7, 13, 9, 0, 0)
        };
    }

    private sealed class TestClock : IClock
    {
        private static readonly DateTime FixedUtcNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedUtcNow.ToLocalTime();
        public DateTime Today => FixedUtcNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
