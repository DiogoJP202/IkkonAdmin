using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class ConfiguracaoQueryService(
    ApplicationDbContext dbContext,
    IClock clock,
    IConfiguracaoSistemaProvider configuracaoProvider) : IConfiguracaoQueryService
{
    public async Task<ConfiguracoesIndexViewModel> ObterPainelAsync(CancellationToken cancellationToken = default)
    {
        var form = await ObterFormularioAsync(cancellationToken);
        var hoje = clock.TodayDate;
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
        var config = await configuracaoProvider.ObterOuCriarAsync(cancellationToken);
        return MapearFormulario(config);
    }

    private static ConfiguracoesFormViewModel MapearFormulario(ConfiguracaoSistema config)
    {
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
}
