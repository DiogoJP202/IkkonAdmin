using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public class AreaAlunoAdminService(
    IClock clock,
    ICurrentUserService currentUserService,
    IAreaAlunoAulasAdminService aulasAdminService,
    IAreaAlunoDocumentoAdminService documentoAdminService,
    IAreaAlunoComunicadoAdminService comunicadoAdminService,
    IAreaAlunoEventoAdminService eventoAdminService,
    IAreaAlunoConquistaAdminService conquistaAdminService) : IAreaAlunoAdminService
{
    public async Task<AreaAlunoAdminDashboardViewModel> ObterDashboardAsync(CancellationToken cancellationToken = default)
    {
        var hoje = clock.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var proximoMes = inicioMes.AddMonths(1);
        var accessScope = ObterEscopoAulas();

        return new AreaAlunoAdminDashboardViewModel
        {
            AulasProximas = await aulasAdminService.ContarAulasProximasAsync(hoje, accessScope, cancellationToken),
            FrequenciasRegistradasMes = await aulasAdminService.ContarFrequenciasRegistradasAsync(inicioMes, proximoMes, accessScope, cancellationToken),
            DocumentosPendentes = await documentoAdminService.ContarDocumentosPendentesAsync(cancellationToken),
            ComunicadosAtivos = await comunicadoAdminService.ContarComunicadosAtivosAsync(cancellationToken),
            EventosProximos = await eventoAdminService.ContarEventosProximosAsync(cancellationToken),
            ConquistasConcedidasMes = await conquistaAdminService.ContarConquistasConcedidasAsync(inicioMes, proximoMes, cancellationToken),
            ProximasAulas = await aulasAdminService.ListarAulasAdminAsync(8, hoje, accessScope, cancellationToken),
            DocumentosRecentes = await documentoAdminService.ListarDocumentosRecentesAsync(8, cancellationToken),
            ComunicadosRecentes = await comunicadoAdminService.ListarComunicadosRecentesAsync(6, cancellationToken)
        };
    }

    public Task<AreaAlunoAulasAdminViewModel> ObterAulasAsync(CancellationToken cancellationToken = default)
    {
        return aulasAdminService.ObterAulasAsync(cancellationToken);
    }

    public Task<OperationResult> CriarHorarioAsync(
        TurmaHorarioFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.CriarHorarioAsync(model, cancellationToken);
    }

    public Task<OperationResult> AtualizarHorarioAsync(
        int id,
        TurmaHorarioFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.AtualizarHorarioAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirHorarioAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.ExcluirHorarioAsync(id, cancellationToken);
    }

    public Task<OperationResult> VincularInstrutorAsync(
        TurmaInstrutorFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.VincularInstrutorAsync(model, cancellationToken);
    }

    public Task<OperationResult> AtualizarInstrutorAsync(
        int id,
        TurmaInstrutorFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.AtualizarInstrutorAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirInstrutorAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.ExcluirInstrutorAsync(id, cancellationToken);
    }

    public Task<OperationResult> CriarAulaAsync(
        AulaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.CriarAulaAsync(model, cancellationToken);
    }

    public Task<OperationResult> AtualizarAulaAsync(
        int id,
        AulaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.AtualizarAulaAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirAulaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.ExcluirAulaAsync(id, cancellationToken);
    }

    public Task<AreaAlunoFrequenciaAdminViewModel> ObterFrequenciaAsync(CancellationToken cancellationToken = default)
    {
        return aulasAdminService.ObterFrequenciaAsync(ObterEscopoAulas(), cancellationToken);
    }

    public Task<AreaAlunoRegistroFrequenciaViewModel?> ObterRegistroFrequenciaAsync(
        int aulaId,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.ObterRegistroFrequenciaAsync(aulaId, ObterEscopoAulas(), cancellationToken);
    }

    public Task<OperationResult> SalvarFrequenciaAsync(
        FrequenciaRegistroPostViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        return aulasAdminService.SalvarFrequenciaAsync(model, ObterEscopoAulas(usuarioId), cancellationToken);
    }

    public Task<AreaAlunoDocumentosAdminViewModel> ObterDocumentosAsync(CancellationToken cancellationToken = default)
    {
        return documentoAdminService.ObterDocumentosAsync(cancellationToken);
    }

    public Task<OperationResult> CriarDocumentoTipoAsync(
        DocumentoTipoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return documentoAdminService.CriarDocumentoTipoAsync(model, cancellationToken);
    }

    public Task<OperationResult> AtualizarDocumentoTipoAsync(
        int id,
        DocumentoTipoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return documentoAdminService.AtualizarDocumentoTipoAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirDocumentoTipoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return documentoAdminService.ExcluirDocumentoTipoAsync(id, cancellationToken);
    }

    public Task<OperationResult> SolicitarDocumentoAsync(
        DocumentoSolicitacaoFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        return documentoAdminService.SolicitarDocumentoAsync(model, usuarioId, cancellationToken);
    }

    public Task<OperationResult> AtualizarDocumentoSolicitacaoAsync(
        int id,
        DocumentoSolicitacaoEdicaoViewModel model,
        CancellationToken cancellationToken = default)
    {
        return documentoAdminService.AtualizarDocumentoSolicitacaoAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirDocumentoSolicitacaoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return documentoAdminService.ExcluirDocumentoSolicitacaoAsync(id, cancellationToken);
    }

    public Task<OperationResult> AvaliarDocumentoAsync(
        DocumentoAvaliacaoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return documentoAdminService.AvaliarDocumentoAsync(model, cancellationToken);
    }

    public Task<AreaAlunoDocumentoDownload?> ObterDocumentoAdminDownloadAsync(
        int envioId,
        CancellationToken cancellationToken = default)
    {
        return documentoAdminService.ObterDocumentoAdminDownloadAsync(envioId, cancellationToken);
    }

    public Task<AreaAlunoComunicadosAdminViewModel> ObterComunicadosAsync(CancellationToken cancellationToken = default)
    {
        return comunicadoAdminService.ObterComunicadosAsync(cancellationToken);
    }

    public Task<OperationResult> CriarComunicadoAsync(
        ComunicadoFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        return comunicadoAdminService.CriarComunicadoAsync(model, usuarioId, cancellationToken);
    }

    public Task<OperationResult> AtualizarComunicadoAsync(
        int id,
        ComunicadoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return comunicadoAdminService.AtualizarComunicadoAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirComunicadoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return comunicadoAdminService.ExcluirComunicadoAsync(id, cancellationToken);
    }

    public Task<AreaAlunoEventosAdminViewModel> ObterEventosAsync(CancellationToken cancellationToken = default)
    {
        return eventoAdminService.ObterEventosAsync(cancellationToken);
    }

    public Task<OperationResult> CriarEventoAsync(
        EventoAlunoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return eventoAdminService.CriarEventoAsync(model, cancellationToken);
    }

    public Task<OperationResult> AtualizarEventoAsync(
        int id,
        EventoAlunoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return eventoAdminService.AtualizarEventoAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirEventoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return eventoAdminService.ExcluirEventoAsync(id, cancellationToken);
    }

    public Task<AreaAlunoConquistasAdminViewModel> ObterConquistasAsync(CancellationToken cancellationToken = default)
    {
        return conquistaAdminService.ObterConquistasAsync(cancellationToken);
    }

    public Task<OperationResult> CriarInsigniaAsync(
        InsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return conquistaAdminService.CriarInsigniaAsync(model, cancellationToken);
    }

    public Task<OperationResult> AtualizarInsigniaAsync(
        int id,
        InsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return conquistaAdminService.AtualizarInsigniaAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirInsigniaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return conquistaAdminService.ExcluirInsigniaAsync(id, cancellationToken);
    }

    public Task<OperationResult> AtribuirInsigniaAsync(
        AlunoInsigniaFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        return conquistaAdminService.AtribuirInsigniaAsync(model, usuarioId, cancellationToken);
    }

    public Task<OperationResult> AtualizarAlunoInsigniaAsync(
        int id,
        AlunoInsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        return conquistaAdminService.AtualizarAlunoInsigniaAsync(id, model, cancellationToken);
    }

    public Task<OperationResult> ExcluirAlunoInsigniaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return conquistaAdminService.ExcluirAlunoInsigniaAsync(id, cancellationToken);
    }

    private AulaAccessScope ObterEscopoAulas(int? usuarioId = null)
    {
        var userId = usuarioId ?? currentUserService.UserId;
        var hasGlobalAccess = currentUserService.IsInRole(Security.AppRoles.Admin) ||
                              currentUserService.HasClaim(
                                  Security.AppClaimTypes.Permissao,
                                  Security.AppPermissions.AreaAlunoManage);

        return new AulaAccessScope(userId, hasGlobalAccess);
    }
}
