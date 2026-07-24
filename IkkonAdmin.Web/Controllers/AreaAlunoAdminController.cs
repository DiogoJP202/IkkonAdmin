using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("admin/area-aluno")]
[Authorize(Policy = AuthorizationPolicies.Funcionario)]
public class AreaAlunoAdminController(
    IAreaAlunoAdminService areaAlunoAdminService,
    ICurrentUserService currentUserService) : Controller
{
    [HttpGet("")]
    [Authorize(Policy = AuthorizationPolicies.AreaAlunoView)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Área do Aluno";
        return View(await areaAlunoAdminService.ObterDashboardAsync(cancellationToken));
    }

    [HttpGet("aulas")]
    [Authorize(Policy = AuthorizationPolicies.AulasView)]
    public async Task<IActionResult> Aulas(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Aulas e horários";
        return View(await areaAlunoAdminService.ObterAulasAsync(cancellationToken));
    }

    [HttpPost("aulas/horarios")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasCreate)]
    public async Task<IActionResult> CriarHorario([Bind(Prefix = "NovoHorario")] TurmaHorarioFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.CriarHorarioAsync(model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados do horário.")));

        return RedirectToAction(nameof(Aulas));
    }

    [HttpPost("aulas/horarios/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasEdit)]
    public async Task<IActionResult> AtualizarHorario(int id, TurmaHorarioFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarHorarioAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados do horário.")));

        return RedirectToAction(nameof(Aulas));
    }

    [HttpPost("aulas/horarios/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasEdit)]
    public async Task<IActionResult> ExcluirHorario(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirHorarioAsync(id, cancellationToken));
        return RedirectToAction(nameof(Aulas));
    }

    [HttpPost("aulas/instrutores")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasCreate)]
    public async Task<IActionResult> VincularInstrutor([Bind(Prefix = "NovoInstrutor")] TurmaInstrutorFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.VincularInstrutorAsync(model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados do instrutor.")));

        return RedirectToAction(nameof(Aulas));
    }

    [HttpPost("aulas/instrutores/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasEdit)]
    public async Task<IActionResult> AtualizarInstrutor(int id, TurmaInstrutorFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarInstrutorAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados do instrutor.")));

        return RedirectToAction(nameof(Aulas));
    }

    [HttpPost("aulas/instrutores/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasEdit)]
    public async Task<IActionResult> ExcluirInstrutor(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirInstrutorAsync(id, cancellationToken));
        return RedirectToAction(nameof(Aulas));
    }

    [HttpPost("aulas")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasCreate)]
    public async Task<IActionResult> CriarAula([Bind(Prefix = "NovaAula")] AulaFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.CriarAulaAsync(model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados da aula.")));

        return RedirectToAction(nameof(Aulas));
    }

    [HttpPost("aulas/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasEdit)]
    public async Task<IActionResult> AtualizarAula(int id, AulaFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarAulaAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados da aula.")));

        return RedirectToAction(nameof(Aulas));
    }

    [HttpPost("aulas/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AulasEdit)]
    public async Task<IActionResult> ExcluirAula(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirAulaAsync(id, cancellationToken));
        return RedirectToAction(nameof(Aulas));
    }

    [HttpGet("frequencia")]
    [Authorize(Policy = AuthorizationPolicies.FrequenciaView)]
    public async Task<IActionResult> Frequencia(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Frequência";
        return View(await areaAlunoAdminService.ObterFrequenciaAsync(cancellationToken));
    }

    [HttpGet("frequencia/{aulaId:int}")]
    [Authorize(Policy = AuthorizationPolicies.FrequenciaCreate)]
    public async Task<IActionResult> RegistroFrequencia(int aulaId, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Registrar frequência";
        var model = await areaAlunoAdminService.ObterRegistroFrequenciaAsync(aulaId, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost("frequencia/{aulaId:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.FrequenciaCreate)]
    public async Task<IActionResult> SalvarFrequencia(int aulaId, FrequenciaRegistroPostViewModel model, CancellationToken cancellationToken)
    {
        model.AulaId = aulaId;
        await ExecutarOperacaoAsync(areaAlunoAdminService.SalvarFrequenciaAsync(model, ObterUsuarioId(), cancellationToken));
        return RedirectToAction(nameof(Frequencia));
    }

    [HttpGet("documentos")]
    [Authorize(Policy = AuthorizationPolicies.DocumentosView)]
    public async Task<IActionResult> Documentos(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Documentos";
        return View(await areaAlunoAdminService.ObterDocumentosAsync(cancellationToken));
    }

    [HttpPost("documentos/tipos")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DocumentosCreate)]
    public async Task<IActionResult> CriarDocumentoTipo([Bind(Prefix = "NovoTipo")] DocumentoTipoFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.CriarDocumentoTipoAsync(model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise o tipo de documento.")));

        return RedirectToAction(nameof(Documentos));
    }

    [HttpPost("documentos/tipos/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DocumentosEdit)]
    public async Task<IActionResult> AtualizarDocumentoTipo(int id, DocumentoTipoFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarDocumentoTipoAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise o tipo de documento.")));

        return RedirectToAction(nameof(Documentos));
    }

    [HttpPost("documentos/tipos/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DocumentosEdit)]
    public async Task<IActionResult> ExcluirDocumentoTipo(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirDocumentoTipoAsync(id, cancellationToken));
        return RedirectToAction(nameof(Documentos));
    }

    [HttpPost("documentos/solicitar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DocumentosCreate)]
    public async Task<IActionResult> SolicitarDocumento([Bind(Prefix = "NovaSolicitacao")] DocumentoSolicitacaoFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.SolicitarDocumentoAsync(model, ObterUsuarioId(), cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise a solicitação de documento.")));

        return RedirectToAction(nameof(Documentos));
    }

    [HttpPost("documentos/solicitacoes/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DocumentosEdit)]
    public async Task<IActionResult> AtualizarDocumentoSolicitacao(int id, DocumentoSolicitacaoEdicaoViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarDocumentoSolicitacaoAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise a solicitação de documento.")));

        return RedirectToAction(nameof(Documentos));
    }

    [HttpPost("documentos/solicitacoes/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DocumentosEdit)]
    public async Task<IActionResult> ExcluirDocumentoSolicitacao(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirDocumentoSolicitacaoAsync(id, cancellationToken));
        return RedirectToAction(nameof(Documentos));
    }

    [HttpPost("documentos/avaliar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DocumentosApprove)]
    public async Task<IActionResult> AvaliarDocumento(DocumentoAvaliacaoFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.AvaliarDocumentoAsync(model, cancellationToken));
        return RedirectToAction(nameof(Documentos));
    }

    [HttpGet("documentos/baixar/{envioId:int}")]
    [Authorize(Policy = AuthorizationPolicies.DocumentosView)]
    public async Task<IActionResult> BaixarDocumento(int envioId, CancellationToken cancellationToken)
    {
        var arquivo = await areaAlunoAdminService.ObterDocumentoAdminDownloadAsync(envioId, cancellationToken);
        if (arquivo is null)
        {
            return NotFound();
        }

        return PhysicalFile(
            arquivo.CaminhoArquivo,
            string.IsNullOrWhiteSpace(arquivo.ContentType) ? "application/octet-stream" : arquivo.ContentType,
            arquivo.NomeArquivoOriginal);
    }

    [HttpGet("comunicados")]
    [Authorize(Policy = AuthorizationPolicies.ComunicadosView)]
    public async Task<IActionResult> Comunicados(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Comunicados";
        return View(await areaAlunoAdminService.ObterComunicadosAsync(cancellationToken));
    }

    [HttpPost("comunicados")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ComunicadosCreate)]
    public async Task<IActionResult> CriarComunicado([Bind(Prefix = "NovoComunicado")] ComunicadoFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.CriarComunicadoAsync(model, ObterUsuarioId(), cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados do comunicado.")));

        return RedirectToAction(nameof(Comunicados));
    }

    [HttpPost("comunicados/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ComunicadosEdit)]
    public async Task<IActionResult> AtualizarComunicado(int id, ComunicadoFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarComunicadoAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados do comunicado.")));

        return RedirectToAction(nameof(Comunicados));
    }

    [HttpPost("comunicados/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ComunicadosDelete)]
    public async Task<IActionResult> ExcluirComunicado(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirComunicadoAsync(id, cancellationToken));
        return RedirectToAction(nameof(Comunicados));
    }

    [HttpGet("eventos")]
    [Authorize(Policy = AuthorizationPolicies.EventosAlunoView)]
    public async Task<IActionResult> Eventos(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Eventos dos alunos";
        return View(await areaAlunoAdminService.ObterEventosAsync(cancellationToken));
    }

    [HttpPost("eventos")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.EventosAlunoCreate)]
    public async Task<IActionResult> CriarEvento([Bind(Prefix = "NovoEvento")] EventoAlunoFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.CriarEventoAsync(model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados do evento.")));

        return RedirectToAction(nameof(Eventos));
    }

    [HttpPost("eventos/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.EventosAlunoEdit)]
    public async Task<IActionResult> AtualizarEvento(int id, EventoAlunoFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarEventoAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados do evento.")));

        return RedirectToAction(nameof(Eventos));
    }

    [HttpPost("eventos/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.EventosAlunoDelete)]
    public async Task<IActionResult> ExcluirEvento(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirEventoAsync(id, cancellationToken));
        return RedirectToAction(nameof(Eventos));
    }

    [HttpGet("conquistas")]
    [Authorize(Policy = AuthorizationPolicies.ConquistasView)]
    public async Task<IActionResult> Conquistas(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Conquistas";
        return View(await areaAlunoAdminService.ObterConquistasAsync(cancellationToken));
    }

    [HttpPost("conquistas/insignias")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConquistasCreate)]
    public async Task<IActionResult> CriarInsignia([Bind(Prefix = "NovaInsignia")] InsigniaFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.CriarInsigniaAsync(model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados da insígnia.")));

        return RedirectToAction(nameof(Conquistas));
    }

    [HttpPost("conquistas/insignias/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConquistasEdit)]
    public async Task<IActionResult> AtualizarInsignia(int id, InsigniaFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarInsigniaAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise os dados da insígnia.")));

        return RedirectToAction(nameof(Conquistas));
    }

    [HttpPost("conquistas/insignias/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConquistasEdit)]
    public async Task<IActionResult> ExcluirInsignia(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirInsigniaAsync(id, cancellationToken));
        return RedirectToAction(nameof(Conquistas));
    }

    [HttpPost("conquistas/atribuir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConquistasCreate)]
    public async Task<IActionResult> AtribuirInsignia([Bind(Prefix = "NovaAtribuicao")] AlunoInsigniaFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtribuirInsigniaAsync(model, ObterUsuarioId(), cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise a atribuição de insígnia.")));

        return RedirectToAction(nameof(Conquistas));
    }

    [HttpPost("conquistas/atribuicoes/{id:int}/editar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConquistasEdit)]
    public async Task<IActionResult> AtualizarAlunoInsignia(int id, AlunoInsigniaFormViewModel model, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(
            ModelState.IsValid
                ? areaAlunoAdminService.AtualizarAlunoInsigniaAsync(id, model, cancellationToken)
                : Task.FromResult(OperationResult.Fail("Revise a conquista.")));

        return RedirectToAction(nameof(Conquistas));
    }

    [HttpPost("conquistas/atribuicoes/{id:int}/excluir")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.ConquistasEdit)]
    public async Task<IActionResult> ExcluirAlunoInsignia(int id, CancellationToken cancellationToken)
    {
        await ExecutarOperacaoAsync(areaAlunoAdminService.ExcluirAlunoInsigniaAsync(id, cancellationToken));
        return RedirectToAction(nameof(Conquistas));
    }

    private async Task ExecutarOperacaoAsync(Task<OperationResult> operacao)
    {
        var resultado = await operacao;
        resultado.AddToTempData(TempData);
    }

    private int? ObterUsuarioId()
    {
        return currentUserService.UserId;
    }
}
