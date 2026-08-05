using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.FinanceiroView)]
public class FinanceiroController(
    IFinanceiroQueryService financeiroQueryService,
    IFinanceiroService financeiroService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? buscaAluno,
        StatusMensalidadeEnum? statusFiltro,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        ViewData["Title"] = "Financeiro";
        tamanhoPagina = NormalizarTamanhoPagina(tamanhoPagina);

        var vm = await financeiroQueryService.ObterResumoAsync(buscaAluno, statusFiltro, pagina, tamanhoPagina, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.FinanceiroCreate)]
    public async Task<IActionResult> GerarMensalidades(
        int anoCompetenciaGeracao,
        int mesCompetenciaGeracao,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        if (anoCompetenciaGeracao is < 2000 or > 2100 || mesCompetenciaGeracao is < 1 or > 12)
        {
            TempData["Error"] = "Competência inválida para geração.";
            return RedirecionarLocal(returnUrl, nameof(Index));
        }

        var resultado = await financeiroService.GerarMensalidadesAsync(anoCompetenciaGeracao, mesCompetenciaGeracao, cancellationToken);

        TempData["Success"] =
            $"Geração concluída para {mesCompetenciaGeracao:D2}/{anoCompetenciaGeracao}. Criadas: {resultado.Criadas}. Já existentes: {resultado.JaExistentes}.";

        return RedirecionarLocal(returnUrl, nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Atrasados(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Mensalidades Atrasadas";
        var vm = await financeiroQueryService.ObterAtrasadosAsync(cancellationToken);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.FinanceiroCreate)]
    public async Task<IActionResult> RegistrarPagamento(int? mensalidadeId, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Registrar Pagamento";

        if (!mensalidadeId.HasValue)
        {
            return View(new RegistrarPagamentoViewModel { ReturnUrl = returnUrl });
        }

        var vm = await financeiroQueryService.ObterFormularioPagamentoAsync(mensalidadeId.Value, cancellationToken);
        if (vm is null)
        {
            return NotFound();
        }

        vm.ReturnUrl = returnUrl;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.FinanceiroCreate)]
    public async Task<IActionResult> RegistrarPagamento(RegistrarPagamentoViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Registrar Pagamento";
            await RecarregarContextoPagamentoAsync(model, cancellationToken);
            return View(model);
        }

        var result = await financeiroService.RegistrarPagamentoAsync(model, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            ViewData["Title"] = "Registrar Pagamento";
            await RecarregarContextoPagamentoAsync(model, cancellationToken);
            return View(model);
        }

        result.AddToTempData(TempData);
        return RedirecionarLocal(model.ReturnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.FinanceiroEdit)]
    public async Task<IActionResult> AtualizarValorFinal(int mensalidadeId, decimal valorFinal, string? returnUrl, CancellationToken cancellationToken)
    {
        var result = await financeiroService.AtualizarValorFinalAsync(mensalidadeId, valorFinal, cancellationToken);
        result.AddToTempData(TempData);

        return RedirecionarLocal(returnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.FinanceiroEdit)]
    public async Task<IActionResult> AlterarStatusMensalidade(
        int mensalidadeId,
        StatusMensalidadeEnum status,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var result = await financeiroService.AlterarStatusMensalidadeAsync(mensalidadeId, status, cancellationToken);
        result.AddToTempData(TempData);

        return RedirecionarLocal(returnUrl, nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> HistoricoAluno(int alunoId, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Histórico Financeiro";
        var vm = await financeiroQueryService.ObterHistoricoAlunoAsync(alunoId, cancellationToken);

        if (vm is null)
        {
            return NotFound();
        }

        return View(vm);
    }

    private async Task RecarregarContextoPagamentoAsync(RegistrarPagamentoViewModel model, CancellationToken cancellationToken)
    {
        var contexto = await financeiroQueryService.ObterFormularioPagamentoAsync(model.MensalidadeId, cancellationToken);
        if (contexto is null)
        {
            return;
        }

        model.AlunoNome ??= contexto.AlunoNome;
        model.Competencia ??= contexto.Competencia;
        model.DataVencimento ??= contexto.DataVencimento;
        model.ValorMensalidadeAtual ??= contexto.ValorMensalidadeAtual;
        model.StatusMensalidadeAtual ??= contexto.StatusMensalidadeAtual;
    }

    private IActionResult RedirecionarLocal(string? returnUrl, string fallbackAction)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(fallbackAction);
    }

    private static int NormalizarTamanhoPagina(int tamanhoPagina)
    {
        return tamanhoPagina is 10 or 20 or 30 ? tamanhoPagina : 20;
    }
}
