using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Services;

public class AreaAlunoService(
    IAreaAlunoContextService contextService,
    IAreaAlunoPerfilService perfilService,
    IAreaAlunoFinanceiroService financeiroService,
    IAreaAlunoTurmasService turmasService,
    IAreaAlunoFrequenciaService frequenciaService,
    IAreaAlunoEventosService eventosService,
    IAreaAlunoDocumentosService documentosService,
    IAreaAlunoComunicadosService comunicadosService,
    IAreaAlunoConquistasService conquistasService) : IAreaAlunoService
{
    public async Task<AreaAlunoDashboardViewModel?> ObterDashboardAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await contextService.ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        var perfil = await perfilService.ObterPerfilBaseAsync(contexto.AlunoId, cancellationToken);
        if (perfil is null)
        {
            return null;
        }

        var mensalidades = await financeiroService.ListarMensalidadesAsync(contexto.AlunoId, 6, cancellationToken);
        var turmas = await turmasService.ListarTurmasAsync(contexto.AlunoId, cancellationToken);
        var resumoFinanceiro = await financeiroService.ObterResumoFinanceiroAsync(contexto.AlunoId, cancellationToken);
        var proximasAulas = await turmasService.ListarProximasAulasAsync(contexto.TurmaIds, 5, cancellationToken);
        var eventos = await eventosService.ListarEventosAsync(contexto.AlunoId, contexto.TurmaIds, 5, cancellationToken);
        var documentos = await documentosService.ListarDocumentosAsync(contexto.AlunoId, 5, cancellationToken);
        var comunicados = await comunicadosService.ListarComunicadosAsync(contexto.AlunoId, contexto.TurmaIds, 5, cancellationToken);
        var frequenciaResumo = await frequenciaService.ObterResumoFrequenciaAsync(contexto.AlunoId, cancellationToken);
        var faltasRecentes = await frequenciaService.ListarFaltasRecentesAsync(contexto.AlunoId, 5, cancellationToken);
        var conquistasRecentes = await conquistasService.ListarConquistasAsync(contexto.AlunoId, 4, cancellationToken);

        var alertas = MontarAlertas(
            resumoFinanceiro.MensalidadesAtrasadas,
            resumoFinanceiro.TotalEmAberto,
            documentos,
            comunicados,
            proximasAulas,
            eventos);

        return new AreaAlunoDashboardViewModel
        {
            AlunoId = contexto.AlunoId,
            NomeCompleto = perfil.NomeCompleto,
            Email = perfil.Email,
            Celular = perfil.Celular,
            FotoPerfilUrl = contexto.FotoPerfilUrl,
            Status = perfil.Status,
            TurmaPrincipal = perfil.TurmaPrincipal,
            DataEntrada = perfil.DataEntrada,
            TotalEmAberto = resumoFinanceiro.TotalEmAberto,
            MensalidadesAtrasadas = resumoFinanceiro.MensalidadesAtrasadas,
            DocumentosPendentes = documentos.Count(x => x.Status is DocumentoStatusEnum.Solicitado or DocumentoStatusEnum.Pendente or DocumentoStatusEnum.Recusado),
            ComunicadosNaoLidos = comunicados.Count(x => !x.Lido),
            FaltasNaoJustificadas = frequenciaResumo.FaltasNaoJustificadas,
            PercentualPresenca = frequenciaResumo.PercentualPresenca,
            Turmas = turmas,
            MensalidadesRecentes = mensalidades,
            ProximasAulas = proximasAulas,
            ProximosEventos = eventos,
            DocumentosRecentes = documentos,
            ComunicadosRecentes = comunicados,
            FaltasRecentes = faltasRecentes,
            ConquistasRecentes = conquistasRecentes,
            Alertas = alertas
        };
    }

    public Task<AreaAlunoPerfilViewModel?> ObterPerfilAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return perfilService.ObterPerfilAsync(usuarioId, cancellationToken);
    }

    public Task<AreaAlunoFinanceiroViewModel?> ObterFinanceiroAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return financeiroService.ObterFinanceiroAsync(usuarioId, cancellationToken);
    }

    public Task<AreaAlunoTurmasViewModel?> ObterTurmasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return turmasService.ObterTurmasAsync(usuarioId, cancellationToken);
    }

    public Task<AreaAlunoAulasViewModel?> ObterAulasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return turmasService.ObterAulasAsync(usuarioId, cancellationToken);
    }

    public Task<AreaAlunoFrequenciaViewModel?> ObterFrequenciaAsync(
        int usuarioId,
        DateOnly? inicio,
        DateOnly? fim,
        CancellationToken cancellationToken = default)
    {
        return frequenciaService.ObterFrequenciaAsync(usuarioId, inicio, fim, cancellationToken);
    }

    public Task<AreaAlunoEventosViewModel?> ObterEventosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return eventosService.ObterEventosAsync(usuarioId, cancellationToken);
    }

    public Task<AreaAlunoDocumentosViewModel?> ObterDocumentosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return documentosService.ObterDocumentosAsync(usuarioId, cancellationToken);
    }

    public Task<OperationResult> EnviarDocumentoAsync(
        int usuarioId,
        int solicitacaoId,
        IFormFile arquivo,
        CancellationToken cancellationToken = default)
    {
        return documentosService.EnviarDocumentoAsync(usuarioId, solicitacaoId, arquivo, cancellationToken);
    }

    public Task<AreaAlunoDocumentoDownload?> ObterDocumentoParaDownloadAsync(
        int usuarioId,
        int envioId,
        CancellationToken cancellationToken = default)
    {
        return documentosService.ObterDocumentoParaDownloadAsync(usuarioId, envioId, cancellationToken);
    }

    public Task<AreaAlunoComunicadosViewModel?> ObterComunicadosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return comunicadosService.ObterComunicadosAsync(usuarioId, cancellationToken);
    }

    public Task<bool> MarcarComunicadoComoLidoAsync(
        int usuarioId,
        int comunicadoId,
        CancellationToken cancellationToken = default)
    {
        return comunicadosService.MarcarComunicadoComoLidoAsync(usuarioId, comunicadoId, cancellationToken);
    }

    public Task<AreaAlunoConquistasViewModel?> ObterConquistasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        return conquistasService.ObterConquistasAsync(usuarioId, cancellationToken);
    }

    private static List<AreaAlunoAlertaViewModel> MontarAlertas(
        int mensalidadesAtrasadas,
        decimal totalEmAberto,
        IReadOnlyCollection<AreaAlunoDocumentoItemViewModel> documentos,
        IReadOnlyCollection<AreaAlunoComunicadoItemViewModel> comunicados,
        IReadOnlyCollection<AreaAlunoAulaItemViewModel> aulas,
        IReadOnlyCollection<AreaAlunoEventoItemViewModel> eventos)
    {
        var alertas = new List<AreaAlunoAlertaViewModel>();

        if (mensalidadesAtrasadas > 0)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "danger",
                Titulo = "Pendência financeira",
                Descricao = $"{mensalidadesAtrasadas} mensalidade(s) em atraso. Total em aberto: {totalEmAberto:C}.",
                Url = "/area-do-aluno/financeiro"
            });
        }

        var documentosPendentes = documentos.Count(x => x.Status is DocumentoStatusEnum.Solicitado or DocumentoStatusEnum.Pendente or DocumentoStatusEnum.Recusado);
        if (documentosPendentes > 0)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "warning",
                Titulo = "Documentos pendentes",
                Descricao = $"{documentosPendentes} documento(s) aguardam envio ou revisão.",
                Url = "/area-do-aluno/documentos"
            });
        }

        var comunicadosNaoLidos = comunicados.Count(x => !x.Lido);
        if (comunicadosNaoLidos > 0)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "info",
                Titulo = "Comunicados novos",
                Descricao = $"{comunicadosNaoLidos} comunicado(s) ainda não foram lidos.",
                Url = "/area-do-aluno/comunicados"
            });
        }

        var proximaAula = aulas.FirstOrDefault();
        if (proximaAula is not null)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "success",
                Titulo = "Próxima aula",
                Descricao = $"{proximaAula.Turma} em {proximaAula.Inicio:dd/MM HH:mm}.",
                Url = "/area-do-aluno/aulas"
            });
        }

        var eventoImportante = eventos.FirstOrDefault(x => x.Importante);
        if (eventoImportante is not null)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "primary",
                Titulo = "Evento importante",
                Descricao = $"{eventoImportante.Titulo} em {eventoImportante.Inicio:dd/MM HH:mm}.",
                Url = "/area-do-aluno/eventos"
            });
        }

        return alertas.Take(5).ToList();
    }
}
