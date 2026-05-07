using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.FinanceiroView)]
public class FinanceiroController(IFinanceiroService financeiroService) : Controller
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

        var vm = await financeiroService.ObterResumoAsync(buscaAluno, statusFiltro, pagina, tamanhoPagina, cancellationToken);
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
        var vm = await financeiroService.ObterAtrasadosAsync(cancellationToken);
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

        var vm = await financeiroService.ObterFormularioPagamentoAsync(mensalidadeId.Value, cancellationToken);
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

        var sucesso = await financeiroService.RegistrarPagamentoAsync(model, cancellationToken);
        if (!sucesso)
        {
            ModelState.AddModelError(string.Empty, "Não foi possível registrar o pagamento para a mensalidade informada.");
            ViewData["Title"] = "Registrar Pagamento";
            await RecarregarContextoPagamentoAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = "Pagamento registrado com sucesso.";
        return RedirecionarLocal(model.ReturnUrl, nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.FinanceiroEdit)]
    public async Task<IActionResult> AtualizarValorFinal(int mensalidadeId, decimal valorFinal, string? returnUrl, CancellationToken cancellationToken)
    {
        if (valorFinal < 0)
        {
            TempData["Error"] = "Valor final não pode ser negativo.";
            return RedirecionarLocal(returnUrl, nameof(Index));
        }

        var atualizado = await financeiroService.AtualizarValorFinalAsync(mensalidadeId, valorFinal, cancellationToken);
        TempData[atualizado ? "Success" : "Error"] = atualizado
            ? "Valor final atualizado."
            : "Mensalidade não encontrada para atualizar valor.";

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
        var atualizado = await financeiroService.AlterarStatusMensalidadeAsync(mensalidadeId, status, cancellationToken);
        TempData[atualizado ? "Success" : "Error"] = atualizado
            ? "Status da mensalidade atualizado."
            : "Mensalidade não encontrada para alterar status.";

        return RedirecionarLocal(returnUrl, nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> HistoricoAluno(int alunoId, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Histórico Financeiro";
        var vm = await financeiroService.ObterHistoricoAlunoAsync(alunoId, cancellationToken);

        if (vm is null)
        {
            return NotFound();
        }

        return View(vm);
    }

    private async Task RecarregarContextoPagamentoAsync(RegistrarPagamentoViewModel model, CancellationToken cancellationToken)
    {
        var contexto = await financeiroService.ObterFormularioPagamentoAsync(model.MensalidadeId, cancellationToken);
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
