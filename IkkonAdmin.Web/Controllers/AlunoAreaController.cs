using System.Security.Claims;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Aluno)]
public class AlunoAreaController(IAreaAlunoService areaAlunoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Área do Aluno";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterDashboardAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Perfil(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Meu Perfil";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterPerfilAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Financeiro(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Minhas Mensalidades";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterFinanceiroAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Turmas(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Minhas Turmas";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterTurmasAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Aulas(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Aulas e Horários";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterAulasAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Frequencia(DateOnly? inicio, DateOnly? fim, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Frequência";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterFrequenciaAsync(usuarioId.Value, inicio, fim, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Eventos(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Eventos";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterEventosAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Documentos(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Documentos";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterDocumentosAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarDocumento(int solicitacaoId, IFormFile arquivo, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var resultado = await areaAlunoService.EnviarDocumentoAsync(usuarioId.Value, solicitacaoId, arquivo, cancellationToken);
        TempData[resultado.Sucesso ? "Success" : "Error"] = resultado.Mensagem;
        return RedirectToAction(nameof(Documentos));
    }

    [HttpGet]
    public async Task<IActionResult> BaixarDocumento(int envioId, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var arquivo = await areaAlunoService.ObterDocumentoParaDownloadAsync(usuarioId.Value, envioId, cancellationToken);
        if (arquivo is null)
        {
            return NotFound();
        }

        return PhysicalFile(
            arquivo.CaminhoArquivo,
            string.IsNullOrWhiteSpace(arquivo.ContentType) ? "application/octet-stream" : arquivo.ContentType,
            arquivo.NomeArquivoOriginal);
    }

    [HttpGet]
    public async Task<IActionResult> Comunicados(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Comunicados";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterComunicadosAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarcarComunicadoLido(int comunicadoId, CancellationToken cancellationToken)
    {
        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        await areaAlunoService.MarcarComunicadoComoLidoAsync(usuarioId.Value, comunicadoId, cancellationToken);
        return RedirectToAction(nameof(Comunicados));
    }

    [HttpGet]
    public async Task<IActionResult> Conquistas(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Conquistas";

        var usuarioId = ObterUsuarioId();
        if (!usuarioId.HasValue)
        {
            return Forbid();
        }

        var model = await areaAlunoService.ObterConquistasAsync(usuarioId.Value, cancellationToken);
        return model is null ? RedirectToAction(nameof(AcessoIndisponivel)) : View(model);
    }

    [HttpGet]
    public IActionResult AcessoIndisponivel()
    {
        ViewData["Title"] = "Acesso indisponível";
        return View();
    }

    private int? ObterUsuarioId()
    {
        var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(valor, out var usuarioId) ? usuarioId : null;
    }
}
