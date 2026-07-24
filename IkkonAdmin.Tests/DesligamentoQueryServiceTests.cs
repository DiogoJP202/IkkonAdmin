using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class DesligamentoQueryServiceTests
{
    [Fact]
    public async Task ListarAsync_AplicaBuscaConfirmacaoECarregaTurmaDoAluno()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Base", Modalidade = "Taiko", Ativa = true };
        var marina = CriarAluno("Marina Tanaka", "11122233344", StatusAlunoEnum.Ativo, turma);
        var kenji = CriarAluno("Kenji Mori", "22233344455", StatusAlunoEnum.Ativo, turma);

        dbContext.AddRange(turma, marina, kenji);
        await dbContext.SaveChangesAsync();

        dbContext.Desligamentos.AddRange(
            CriarDesligamento(marina.Id, new DateOnly(2026, 7, 10), confirmacao: null),
            CriarDesligamento(kenji.Id, new DateOnly(2026, 7, 12), confirmacao: new DateOnly(2026, 7, 13)));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var desligamentos = await service.ListarAsync("Marina", confirmado: false);

        var desligamento = Assert.Single(desligamentos);
        Assert.Equal(marina.Id, desligamento.AlunoId);
        Assert.Equal("Marina Tanaka", desligamento.Aluno?.NomeCompleto);
        Assert.Equal("Taiko Base", desligamento.Aluno?.Turma?.Nome);
    }

    [Fact]
    public async Task ListarAlunosElegiveisAsync_RetornaAtivosEInativosOrdenados()
    {
        await using var dbContext = CriarDbContext();
        var turma = new Turma { Nome = "Taiko Base", Modalidade = "Taiko", Ativa = true };

        dbContext.AddRange(
            turma,
            CriarAluno("Bruno Dias", "33344455566", StatusAlunoEnum.Desligado, turma),
            CriarAluno("Ana Mori", "44455566677", StatusAlunoEnum.Inativo, turma),
            CriarAluno("Kenji Mori", "55566677788", StatusAlunoEnum.Ativo, turma));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var alunos = await service.ListarAlunosElegiveisAsync();

        Assert.Collection(
            alunos,
            aluno => Assert.Equal("Ana Mori", aluno.NomeCompleto),
            aluno => Assert.Equal("Kenji Mori", aluno.NomeCompleto));
    }

    [Fact]
    public async Task CalcularPendenciasAsync_SomaAtrasadasEPendentesVencidas()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno("Rafael Sato", "66677788899", StatusAlunoEnum.Ativo, null);
        dbContext.Alunos.Add(aluno);
        await dbContext.SaveChangesAsync();

        dbContext.Mensalidades.AddRange(
            CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Atrasado, new DateOnly(2026, 6, 10), 220.456m),
            CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pendente, new DateOnly(2026, 7, 13), 260m),
            CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pendente, new DateOnly(2026, 8, 10), 300m),
            CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Pago, new DateOnly(2026, 7, 10), 999m),
            CriarMensalidade(aluno.Id, StatusMensalidadeEnum.Cancelado, new DateOnly(2026, 7, 10), 999m));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var total = await service.CalcularPendenciasAsync(aluno.Id);

        Assert.Equal(480.46m, total);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static DesligamentoQueryService CriarService(ApplicationDbContext dbContext)
    {
        return new DesligamentoQueryService(dbContext, new TestClock());
    }

    private static Aluno CriarAluno(string nome, string cpf, StatusAlunoEnum status, Turma? turma)
    {
        return new Aluno
        {
            NomeCompleto = nome,
            CPF = cpf,
            DataEntrada = new DateOnly(2026, 1, 1),
            Status = status,
            Turma = turma
        };
    }

    private static Desligamento CriarDesligamento(
        int alunoId,
        DateOnly dataSolicitacao,
        DateOnly? confirmacao)
    {
        return new Desligamento
        {
            AlunoId = alunoId,
            DataSolicitacao = dataSolicitacao,
            Motivo = "Solicitado pelo aluno",
            DataConfirmacao = confirmacao
        };
    }

    private static Mensalidade CriarMensalidade(
        int alunoId,
        StatusMensalidadeEnum status,
        DateOnly vencimento,
        decimal valor)
    {
        return new Mensalidade
        {
            AlunoId = alunoId,
            Competencia = new DateOnly(vencimento.Year, vencimento.Month, 1),
            DataVencimento = vencimento,
            ValorBase = valor,
            ValorFinal = valor,
            Status = status
        };
    }

    private sealed class TestClock : IClock
    {
        private static readonly DateTime FixedNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Local);

        public DateTime UtcNow => FixedNow.ToUniversalTime();
        public DateTime Now => FixedNow;
        public DateTime Today => FixedNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
