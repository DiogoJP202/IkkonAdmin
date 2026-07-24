using System.Security.Claims;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("admin/agenda")]
[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.GoogleAgendaView)]
public class GoogleAgendaController(
    IGoogleAgendaService googleAgendaService,
    ICurrentUserService currentUserService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GoogleAgendaFiltroViewModel filtro, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Google Agenda";
        AplicarIntervaloDaVisualizacao(filtro);

        if (!ModelState.IsValid)
        {
            return View(new GoogleAgendaIndexViewModel
            {
                Filtro = filtro,
                CalendarId = googleAgendaService.CalendarId,
                OAuthConectado = await googleAgendaService.PossuiConexaoOAuthAsync(cancellationToken)
            });
        }

        try
        {
            var eventos = await googleAgendaService.ListarEventosAsync(filtro, cancellationToken);
            return View(new GoogleAgendaIndexViewModel
            {
                Filtro = filtro,
                Eventos = eventos.ToList(),
                CalendarId = googleAgendaService.CalendarId,
                OAuthConectado = await googleAgendaService.PossuiConexaoOAuthAsync(cancellationToken)
            });
        }
        catch (GoogleAgendaConfigurationException ex)
        {
            return View(new GoogleAgendaIndexViewModel
            {
                Filtro = filtro,
                ConfiguracaoPendente = true,
                MensagemConfiguracao = ex.Message,
                CalendarId = googleAgendaService.CalendarId,
                OAuthConectado = await googleAgendaService.PossuiConexaoOAuthAsync(cancellationToken)
            });
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return View(new GoogleAgendaIndexViewModel
            {
                Filtro = filtro,
                CalendarId = googleAgendaService.CalendarId,
                OAuthConectado = await googleAgendaService.PossuiConexaoOAuthAsync(cancellationToken)
            });
        }
    }

    [HttpGet("google/conectar")]
    [Authorize(Policy = AuthorizationPolicies.GoogleAgendaManage)]
    public async Task<IActionResult> Connect(CancellationToken cancellationToken)
    {
        var state = Guid.NewGuid().ToString("N");
        TempData["GoogleAgendaOAuthState"] = state;

        var callbackUrl = Url.Action(nameof(Callback), "GoogleAgenda", null, Request.Scheme)
            ?? throw new InvalidOperationException("Não foi possível montar a URL de retorno OAuth.");

        var authorizationUrl = await googleAgendaService.GerarUrlAutorizacaoAsync(callbackUrl, state, cancellationToken);
        return Redirect(authorizationUrl);
    }

    [HttpGet("google/callback")]
    [Authorize(Policy = AuthorizationPolicies.GoogleAgendaManage)]
    public async Task<IActionResult> Callback(string? code, string? state, string? error, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(error))
        {
            TempData["Error"] = $"Google recusou a autorização: {error}";
            return RedirectToAction(nameof(Index));
        }

        var expectedState = TempData["GoogleAgendaOAuthState"] as string;
        if (string.IsNullOrWhiteSpace(expectedState) || !string.Equals(expectedState, state, StringComparison.Ordinal))
        {
            TempData["Error"] = "Estado OAuth inválido. Tente conectar novamente.";
            return RedirectToAction(nameof(Index));
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            TempData["Error"] = "Código OAuth não recebido do Google.";
            return RedirectToAction(nameof(Index));
        }

        var callbackUrl = Url.Action(nameof(Callback), "GoogleAgenda", null, Request.Scheme)
            ?? throw new InvalidOperationException("Não foi possível montar a URL de retorno OAuth.");

        try
        {
            await googleAgendaService.ConcluirAutorizacaoOAuthAsync(code, callbackUrl, ObterUsuarioId(), cancellationToken);
            TempData["Success"] = "Google Agenda conectado com sucesso.";
        }
        catch (Exception ex) when (ex is GoogleAgendaConfigurationException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("google/desconectar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.GoogleAgendaManage)]
    public async Task<IActionResult> Disconnect(CancellationToken cancellationToken)
    {
        await googleAgendaService.DesconectarOAuthAsync(ObterUsuarioId(), cancellationToken);
        TempData["Success"] = "Conexão com Google Agenda removida.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("detalhes/{eventoId}")]
    public async Task<IActionResult> Details(string eventoId, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Detalhes do evento";
        var evento = await googleAgendaService.ObterEventoAsync(eventoId, cancellationToken);
        return evento is null ? NotFound() : View(evento);
    }

    [HttpGet("criar")]
    [Authorize(Policy = AuthorizationPolicies.GoogleAgendaCreate)]
    public IActionResult Create()
    {
        ViewData["Title"] = "Novo evento";
        return View(new GoogleAgendaEventoFormViewModel());
    }

    [HttpPost("criar")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.GoogleAgendaCreate)]
    public async Task<IActionResult> Create(GoogleAgendaEventoFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo evento";
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var evento = await googleAgendaService.CriarEventoAsync(model, cancellationToken);
            TempData["Success"] = "Evento criado no Google Agenda.";
            return RedirectToAction(nameof(Details), new { eventoId = evento.Id });
        }
        catch (Exception ex) when (ex is GoogleAgendaConfigurationException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet("editar/{eventoId}")]
    [Authorize(Policy = AuthorizationPolicies.GoogleAgendaEdit)]
    public async Task<IActionResult> Edit(string eventoId, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar evento";
        var evento = await googleAgendaService.ObterEventoAsync(eventoId, cancellationToken);
        if (evento is null)
        {
            return NotFound();
        }

        return View(new GoogleAgendaEventoFormViewModel
        {
            Id = evento.Id,
            Titulo = evento.Titulo,
            Tipo = evento.Tipo,
            Inicio = evento.Inicio,
            Fim = evento.Fim,
            Local = evento.Local,
            Descricao = evento.Descricao
        });
    }

    [HttpPost("editar/{eventoId}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.GoogleAgendaEdit)]
    public async Task<IActionResult> Edit(string eventoId, GoogleAgendaEventoFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar evento";
        if (model.Id != eventoId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await googleAgendaService.AtualizarEventoAsync(eventoId, model, cancellationToken);
            TempData["Success"] = "Evento atualizado no Google Agenda.";
            return RedirectToAction(nameof(Details), new { eventoId });
        }
        catch (Exception ex) when (ex is GoogleAgendaConfigurationException or InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost("excluir/{eventoId}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.GoogleAgendaDelete)]
    public async Task<IActionResult> Delete(string eventoId, CancellationToken cancellationToken)
    {
        try
        {
            await googleAgendaService.ExcluirEventoAsync(eventoId, cancellationToken);
            TempData["Success"] = "Evento excluído do Google Agenda.";
        }
        catch (Exception ex) when (ex is GoogleAgendaConfigurationException or InvalidOperationException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private int? ObterUsuarioId()
    {
        return currentUserService.UserId;
    }

    private static void AplicarIntervaloDaVisualizacao(GoogleAgendaFiltroViewModel filtro)
    {
        if (filtro.Visualizacao != GoogleAgendaVisualizacaoEnum.CalendarioAnual)
        {
            return;
        }

        filtro.Inicio = new DateOnly(filtro.Ano, 1, 1);
        filtro.Fim = new DateOnly(filtro.Ano, 12, 31);
    }
}
