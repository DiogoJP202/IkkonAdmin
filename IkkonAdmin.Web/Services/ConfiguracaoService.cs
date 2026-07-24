using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public class ConfiguracaoService(
    ApplicationDbContext dbContext,
    IClock clock,
    IConfiguracaoSistemaProvider configuracaoProvider,
    IConfiguracaoQueryService queryService) : IConfiguracaoService
{
    public Task<ConfiguracoesIndexViewModel> ObterPainelAsync(CancellationToken cancellationToken = default)
    {
        return queryService.ObterPainelAsync(cancellationToken);
    }

    public Task<ConfiguracoesFormViewModel> ObterFormularioAsync(CancellationToken cancellationToken = default)
    {
        return queryService.ObterFormularioAsync(cancellationToken);
    }

    public async Task SalvarAsync(ConfiguracoesFormViewModel form, CancellationToken cancellationToken = default)
    {
        var config = await configuracaoProvider.ObterOuCriarAsync(cancellationToken);

        config.NomeEscola = LimparOuDefault(form.NomeEscola, "Escola de Taiko Ikkon");
        config.EmailFinanceiro = LimparOpcional(form.EmailFinanceiro);
        config.TelefoneContato = LimparOpcional(form.TelefoneContato);
        config.ValorMensalidadePadrao = decimal.Round(form.ValorMensalidadePadrao, 2);
        config.DiaVencimentoPadrao = Math.Clamp(form.DiaVencimentoPadrao, 1, 28);
        config.DiasToleranciaAtraso = Math.Clamp(form.DiasToleranciaAtraso, 0, 15);
        config.PercentualMultaAtraso = decimal.Round(Math.Clamp(form.PercentualMultaAtraso, 0m, 50m), 2);
        config.PercentualJurosMes = decimal.Round(Math.Clamp(form.PercentualJurosMes, 0m, 20m), 2);
        config.AplicarMultaJurosAutomaticamente = form.AplicarMultaJurosAutomaticamente;
        config.GerarMensalidadesAutomaticamente = form.GerarMensalidadesAutomaticamente;
        config.EnviarLembreteCobranca = form.EnviarLembreteCobranca;
        config.DiasAntecedenciaLembrete = Math.Clamp(form.DiasAntecedenciaLembrete, 0, 30);
        config.MensagemBoasVindasPadrao = LimparOpcional(form.MensagemBoasVindasPadrao);
        config.ChecklistAdmissaoPadrao = LimparOpcional(form.ChecklistAdmissaoPadrao);
        config.PermitirDesligamentoComPendencia = form.PermitirDesligamentoComPendencia;
        config.AtualizarNivelAutomaticamenteNaGraduacao = form.AtualizarNivelAutomaticamenteNaGraduacao;
        config.UltimaAtualizacaoUtc = clock.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RestaurarPadraoAsync(CancellationToken cancellationToken = default)
    {
        var config = await configuracaoProvider.ObterOuCriarAsync(cancellationToken);

        config.NomeEscola = "Escola de Taiko Ikkon";
        config.EmailFinanceiro = null;
        config.TelefoneContato = null;
        config.ValorMensalidadePadrao = 260m;
        config.DiaVencimentoPadrao = 10;
        config.DiasToleranciaAtraso = 0;
        config.PercentualMultaAtraso = 2m;
        config.PercentualJurosMes = 1m;
        config.AplicarMultaJurosAutomaticamente = false;
        config.GerarMensalidadesAutomaticamente = false;
        config.EnviarLembreteCobranca = true;
        config.DiasAntecedenciaLembrete = 3;
        config.MensagemBoasVindasPadrao = null;
        config.ChecklistAdmissaoPadrao = null;
        config.PermitirDesligamentoComPendencia = true;
        config.AtualizarNivelAutomaticamenteNaGraduacao = true;
        config.UltimaAtualizacaoUtc = clock.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<ConfiguracaoSistema> ObterOuCriarAsync(CancellationToken cancellationToken = default)
    {
        return configuracaoProvider.ObterOuCriarAsync(cancellationToken);
    }

    private static string? LimparOpcional(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private static string LimparOuDefault(string? valor, string fallback)
    {
        var texto = LimparOpcional(valor);
        return texto ?? fallback;
    }
}

