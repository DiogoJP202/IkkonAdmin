using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.DesligamentosView)]
public class DesligamentosController(IDesligamentoService desligamentoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, bool? confirmado, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Desligamentos";

        var desligamentos = await desligamentoService.ListarAsync(busca, confirmado, cancellationToken);

        var vm = new DesligamentoIndexViewModel
        {
            Busca = busca,
            Confirmado = confirmado,
            Desligamentos = desligamentos
                .Select(x => new DesligamentoListItemViewModel
                {
                    Id = x.Id,
                    AlunoId = x.AlunoId,
                    AlunoNome = x.Aluno?.NomeCompleto ?? $"Aluno #{x.AlunoId}",
                    StatusAluno = x.Aluno?.Status ?? Enums.StatusAlunoEnum.Desligado,
                    DataSolicitacao = x.DataSolicitacao,
                    DataConfirmacao = x.DataConfirmacao,
                    PendenciaFinanceira = x.PendenciaFinanceira,
                    MultaRescisoria = x.MultaRescisoria,
                    RequerimentoRecebido = x.RequerimentoRecebido,
                    AcessosRemovidos = x.AcessosRemovidos
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.DesligamentosCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo Desligamento";

        var vm = new DesligamentoCreateViewModel();
        await PopularAlunosDisponiveisAsync(vm, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DesligamentosCreate)]
    public async Task<IActionResult> Create(DesligamentoCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Novo Desligamento";
            await PopularAlunosDisponiveisAsync(model, cancellationToken);
            return View(model);
        }

        if (model.CalcularPendenciasAutomaticamente)
        {
            model.PendenciaFinanceira = await desligamentoService.CalcularPendenciasAsync(model.AlunoId, cancellationToken);
        }

        var desligamento = new Desligamento
        {
            AlunoId = model.AlunoId,
            DataSolicitacao = model.DataSolicitacao,
            Motivo = model.Motivo,
            PendenciaFinanceira = model.PendenciaFinanceira,
            MultaRescisoria = model.MultaRescisoria,
            RequerimentoRecebido = model.RequerimentoRecebido,
            AcessosRemovidos = model.AcessosRemovidos,
            Observacoes = model.Observacoes
        };

        try
        {
            var id = await desligamentoService.CriarAsync(desligamento, cancellationToken);
            TempData["Success"] = "Solicitação de desligamento criada com sucesso.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewData["Title"] = "Novo Desligamento";
            await PopularAlunosDisponiveisAsync(model, cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Detalhes do Desligamento";

        var vm = await ConstruirDetalhesAsync(id, cancellationToken);
        if (vm is null)
        {
            return NotFound();
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DesligamentosEdit)]
    public async Task<IActionResult> RecalcularPendencias(int id, string? returnUrl, CancellationToken cancellationToken)
    {
        var detalhes = await desligamentoService.ObterDetalhesAsync(id, cancellationToken);
        if (detalhes is null)
        {
            return NotFound();
        }

        var novoTotal = await desligamentoService.CalcularPendenciasAsync(detalhes.AlunoId, cancellationToken);

        await desligamentoService.AtualizarAsync(
            id,
            detalhes.Motivo,
            novoTotal,
            detalhes.MultaRescisoria,
            detalhes.RequerimentoRecebido,
            detalhes.AcessosRemovidos,
            detalhes.Observacoes,
            cancellationToken);

        TempData["Success"] = $"Pendências recalculadas: {novoTotal:C}.";
        return RedirecionarLocal(returnUrl, id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DesligamentosEdit)]
    public async Task<IActionResult> Atualizar(int id, DesligamentoDetalhesViewModel model, string? returnUrl, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Detalhes do Desligamento";
            return View("Details", model);
        }

        var atualizado = await desligamentoService.AtualizarAsync(
            id,
            model.Motivo,
            model.PendenciaFinanceira,
            model.MultaRescisoria,
            model.RequerimentoRecebido,
            model.AcessosRemovidos,
            model.Observacoes,
            cancellationToken);

        TempData[atualizado ? "Success" : "Error"] = atualizado
            ? "Processo de desligamento atualizado."
            : "Não foi possível atualizar o desligamento.";

        return RedirecionarLocal(returnUrl, id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.DesligamentosEdit)]
    public async Task<IActionResult> Confirmar(int id, bool encerrarCobrancasFuturas, string? returnUrl, CancellationToken cancellationToken)
    {
        var resultado = await desligamentoService.ConfirmarAsync(id, encerrarCobrancasFuturas, cancellationToken);

        if (!resultado.Sucesso)
        {
            TempData["Error"] = resultado.Erro ?? "Não foi possível confirmar o desligamento.";
            return RedirecionarLocal(returnUrl, id);
        }

        TempData["Success"] = $"Desligamento confirmado. Cobranças futuras canceladas: {resultado.CobrancasCanceladas}.";
        return RedirecionarLocal(returnUrl, id);
    }

    private async Task PopularAlunosDisponiveisAsync(DesligamentoCreateViewModel model, CancellationToken cancellationToken)
    {
        var alunos = await desligamentoService.ListarAlunosElegiveisAsync(cancellationToken);
        model.AlunosDisponiveis = alunos
            .Select(x => new DesligamentoAlunoOpcaoViewModel
            {
                Id = x.Id,
                Nome = x.NomeCompleto,
                Status = x.Status,
                Turma = x.Turma?.Nome
            })
            .ToList();
    }

    private async Task<DesligamentoDetalhesViewModel?> ConstruirDetalhesAsync(int id, CancellationToken cancellationToken)
    {
        var desligamento = await desligamentoService.ObterDetalhesAsync(id, cancellationToken);
        if (desligamento is null)
        {
            return null;
        }

        return new DesligamentoDetalhesViewModel
        {
            Id = desligamento.Id,
            AlunoId = desligamento.AlunoId,
            AlunoNome = desligamento.Aluno?.NomeCompleto ?? $"Aluno #{desligamento.AlunoId}",
            StatusAluno = desligamento.Aluno?.Status ?? Enums.StatusAlunoEnum.Desligado,
            DataSolicitacao = desligamento.DataSolicitacao,
            DataConfirmacao = desligamento.DataConfirmacao,
            Motivo = desligamento.Motivo,
            PendenciaFinanceira = desligamento.PendenciaFinanceira,
            MultaRescisoria = desligamento.MultaRescisoria,
            RequerimentoRecebido = desligamento.RequerimentoRecebido,
            AcessosRemovidos = desligamento.AcessosRemovidos,
            Observacoes = desligamento.Observacoes
        };
    }

    private IActionResult RedirecionarLocal(string? returnUrl, int id)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
