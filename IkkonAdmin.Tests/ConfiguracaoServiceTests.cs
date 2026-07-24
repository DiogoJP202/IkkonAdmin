using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class ConfiguracaoServiceTests
{
    [Fact]
    public async Task SalvarAsync_NormalizaCamposELimites()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        await service.SalvarAsync(new ConfiguracoesFormViewModel
        {
            NomeEscola = "  Ikkon Dojo  ",
            EmailFinanceiro = " financeiro@example.com ",
            TelefoneContato = " (11) 99999-0000 ",
            ValorMensalidadePadrao = 260.456m,
            DiaVencimentoPadrao = 99,
            DiasToleranciaAtraso = 99,
            PercentualMultaAtraso = 99m,
            PercentualJurosMes = 99m,
            AplicarMultaJurosAutomaticamente = true,
            GerarMensalidadesAutomaticamente = true,
            EnviarLembreteCobranca = false,
            DiasAntecedenciaLembrete = 99,
            MensagemBoasVindasPadrao = "  bem-vindo  ",
            ChecklistAdmissaoPadrao = "  checklist  ",
            PermitirDesligamentoComPendencia = false,
            AtualizarNivelAutomaticamenteNaGraduacao = false
        });

        var config = await dbContext.ConfiguracoesSistema.SingleAsync();

        Assert.Equal("Ikkon Dojo", config.NomeEscola);
        Assert.Equal("financeiro@example.com", config.EmailFinanceiro);
        Assert.Equal("(11) 99999-0000", config.TelefoneContato);
        Assert.Equal(260.46m, config.ValorMensalidadePadrao);
        Assert.Equal(28, config.DiaVencimentoPadrao);
        Assert.Equal(15, config.DiasToleranciaAtraso);
        Assert.Equal(50m, config.PercentualMultaAtraso);
        Assert.Equal(20m, config.PercentualJurosMes);
        Assert.True(config.AplicarMultaJurosAutomaticamente);
        Assert.True(config.GerarMensalidadesAutomaticamente);
        Assert.False(config.EnviarLembreteCobranca);
        Assert.Equal(30, config.DiasAntecedenciaLembrete);
        Assert.Equal("bem-vindo", config.MensagemBoasVindasPadrao);
        Assert.Equal("checklist", config.ChecklistAdmissaoPadrao);
        Assert.False(config.PermitirDesligamentoComPendencia);
        Assert.False(config.AtualizarNivelAutomaticamenteNaGraduacao);
        Assert.Equal(TestClock.FixedUtcNow, config.UltimaAtualizacaoUtc);
    }

    [Fact]
    public async Task RestaurarPadraoAsync_ReverteValoresPadraoEAtualizaTimestamp()
    {
        await using var dbContext = CriarDbContext();
        dbContext.ConfiguracoesSistema.Add(new ConfiguracaoSistema
        {
            NomeEscola = "Outro nome",
            EmailFinanceiro = "financeiro@example.com",
            TelefoneContato = "123",
            ValorMensalidadePadrao = 999m,
            DiaVencimentoPadrao = 20,
            DiasToleranciaAtraso = 10,
            PercentualMultaAtraso = 10m,
            PercentualJurosMes = 10m,
            AplicarMultaJurosAutomaticamente = true,
            GerarMensalidadesAutomaticamente = true,
            EnviarLembreteCobranca = false,
            DiasAntecedenciaLembrete = 20,
            MensagemBoasVindasPadrao = "msg",
            ChecklistAdmissaoPadrao = "checklist",
            PermitirDesligamentoComPendencia = false,
            AtualizarNivelAutomaticamenteNaGraduacao = false,
            UltimaAtualizacaoUtc = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        await service.RestaurarPadraoAsync();

        var config = await dbContext.ConfiguracoesSistema.SingleAsync();

        Assert.Equal("Escola de Taiko Ikkon", config.NomeEscola);
        Assert.Null(config.EmailFinanceiro);
        Assert.Null(config.TelefoneContato);
        Assert.Equal(260m, config.ValorMensalidadePadrao);
        Assert.Equal(10, config.DiaVencimentoPadrao);
        Assert.Equal(0, config.DiasToleranciaAtraso);
        Assert.Equal(2m, config.PercentualMultaAtraso);
        Assert.Equal(1m, config.PercentualJurosMes);
        Assert.False(config.AplicarMultaJurosAutomaticamente);
        Assert.False(config.GerarMensalidadesAutomaticamente);
        Assert.True(config.EnviarLembreteCobranca);
        Assert.Equal(3, config.DiasAntecedenciaLembrete);
        Assert.Null(config.MensagemBoasVindasPadrao);
        Assert.Null(config.ChecklistAdmissaoPadrao);
        Assert.True(config.PermitirDesligamentoComPendencia);
        Assert.True(config.AtualizarNivelAutomaticamenteNaGraduacao);
        Assert.Equal(TestClock.FixedUtcNow, config.UltimaAtualizacaoUtc);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static ConfiguracaoService CriarService(ApplicationDbContext dbContext)
    {
        var clock = new TestClock();
        var provider = new ConfiguracaoSistemaProvider(dbContext, clock);
        var queryService = new ConfiguracaoQueryService(dbContext, clock, provider);

        return new ConfiguracaoService(dbContext, clock, provider, queryService);
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
