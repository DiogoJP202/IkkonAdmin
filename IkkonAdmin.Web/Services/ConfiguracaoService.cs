using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class ConfiguracaoService(ApplicationDbContext dbContext) : IConfiguracaoService
{
    public async Task<ConfiguracoesIndexViewModel> ObterPainelAsync(CancellationToken cancellationToken = default)
    {
        var form = await ObterFormularioAsync(cancellationToken);
        var hoje = DateOnly.FromDateTime(DateTime.Today);
        var limite = hoje.AddDays(30);

        var resumo = new ConfiguracoesResumoViewModel
        {
            AlunosAtivos = await dbContext.Alunos.CountAsync(x => x.Status == StatusAlunoEnum.Ativo, cancellationToken),
            TurmasAtivas = await dbContext.Turmas.CountAsync(x => x.Ativa, cancellationToken),
            MensalidadesAtrasadas = await dbContext.Mensalidades.CountAsync(x => x.Status == StatusMensalidadeEnum.Atrasado, cancellationToken),
            DesligamentosEmAberto = await dbContext.Desligamentos.CountAsync(x => !x.DataConfirmacao.HasValue, cancellationToken),
            ExamesProximos30Dias = await dbContext.ExamesGraduacao.CountAsync(x => x.DataExame >= hoje && x.DataExame <= limite, cancellationToken)
        };

        return new ConfiguracoesIndexViewModel
        {
            Form = form,
            Resumo = resumo
        };
    }

    public async Task<ConfiguracoesFormViewModel> ObterFormularioAsync(CancellationToken cancellationToken = default)
    {
        var config = await ObterOuCriarAsync(cancellationToken);

        return new ConfiguracoesFormViewModel
        {
            NomeEscola = config.NomeEscola,
            EmailFinanceiro = config.EmailFinanceiro,
            TelefoneContato = config.TelefoneContato,
            ValorMensalidadePadrao = config.ValorMensalidadePadrao,
            DiaVencimentoPadrao = config.DiaVencimentoPadrao,
            DiasToleranciaAtraso = config.DiasToleranciaAtraso,
            PercentualMultaAtraso = config.PercentualMultaAtraso,
            PercentualJurosMes = config.PercentualJurosMes,
            AplicarMultaJurosAutomaticamente = config.AplicarMultaJurosAutomaticamente,
            GerarMensalidadesAutomaticamente = config.GerarMensalidadesAutomaticamente,
            EnviarLembreteCobranca = config.EnviarLembreteCobranca,
            DiasAntecedenciaLembrete = config.DiasAntecedenciaLembrete,
            MensagemBoasVindasPadrao = config.MensagemBoasVindasPadrao,
            ChecklistAdmissaoPadrao = config.ChecklistAdmissaoPadrao,
            PermitirDesligamentoComPendencia = config.PermitirDesligamentoComPendencia,
            AtualizarNivelAutomaticamenteNaGraduacao = config.AtualizarNivelAutomaticamenteNaGraduacao,
            UltimaAtualizacaoUtc = config.UltimaAtualizacaoUtc
        };
    }

    public async Task SalvarAsync(ConfiguracoesFormViewModel form, CancellationToken cancellationToken = default)
    {
        var config = await ObterOuCriarAsync(cancellationToken);

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
        config.UltimaAtualizacaoUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RestaurarPadraoAsync(CancellationToken cancellationToken = default)
    {
        var config = await ObterOuCriarAsync(cancellationToken);

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
        config.UltimaAtualizacaoUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<ConfiguracaoSistema> ObterOuCriarAsync(CancellationToken cancellationToken = default)
    {
        var config = await dbContext.ConfiguracoesSistema.FirstOrDefaultAsync(cancellationToken);
        if (config is not null)
        {
            return config;
        }

        config = new ConfiguracaoSistema();
        await dbContext.ConfiguracoesSistema.AddAsync(config, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return config;
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

