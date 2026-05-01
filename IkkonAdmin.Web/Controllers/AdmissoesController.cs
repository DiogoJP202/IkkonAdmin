using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.AdmissoesView)]
public class AdmissoesController(IAdmissaoService admissaoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, StatusAdmissaoEnum? status, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Admissões";

        var admissoes = await admissaoService.ListarAsync(busca, status, cancellationToken);

        var vm = new AdmissaoIndexViewModel
        {
            Busca = busca,
            Status = status,
            Admissoes = admissoes
                .Select(x => new AdmissaoListItemViewModel
                {
                    Id = x.Id,
                    NomeInteressado = x.NomeInteressado,
                    DataAulaExperimental = x.DataAulaExperimental,
                    DataMatricula = x.DataMatricula,
                    Status = x.Status,
                    ContratoAssinado = x.ContratoAssinado,
                    PagamentoInicialConfirmado = x.PagamentoInicialConfirmado,
                    IntegracaoConcluida = x.IntegracaoConcluida,
                    AlunoId = x.AlunoId,
                    AlunoNome = x.Aluno?.NomeCompleto
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdmissoesCreate)]
    public IActionResult Create()
    {
        ViewData["Title"] = "Nova Admissão";
        return View(new AdmissaoViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdmissoesCreate)]
    public async Task<IActionResult> Create(AdmissaoViewModel model, CancellationToken cancellationToken)
    {
        if (model.Status == StatusAdmissaoEnum.Matriculado)
        {
            ModelState.AddModelError(nameof(model.Status), "Use o status Matriculado somente após criar a matrícula.");
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Nova Admissão";
            return View(model);
        }

        var admissao = new Admissao
        {
            NomeInteressado = model.NomeInteressado,
            DataAulaExperimental = model.DataAulaExperimental,
            Status = model.Status,
            ContratoAssinado = model.ContratoAssinado,
            PagamentoInicialConfirmado = model.PagamentoInicialConfirmado,
            IntegracaoConcluida = model.IntegracaoConcluida,
            ChecklistObservacoes = model.ChecklistObservacoes
        };

        var id = await admissaoService.CriarAsync(admissao, cancellationToken);

        TempData["Success"] = "Admissão registrada com sucesso.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Detalhes da Admissão";
        var vm = await ConstruirDetalhesViewModelAsync(id, new AdmissaoMatriculaViewModel(), cancellationToken);

        if (vm is null)
        {
            return NotFound();
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdmissoesEdit)]
    public async Task<IActionResult> AtualizarProcesso(
        int id,
        StatusAdmissaoEnum status,
        bool contratoAssinado,
        bool pagamentoInicialConfirmado,
        bool integracaoConcluida,
        string? checklistObservacoes,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var admissao = await admissaoService.ObterDetalhesAsync(id, cancellationToken);
        if (admissao is null)
        {
            return NotFound();
        }

        if (status == StatusAdmissaoEnum.Matriculado && !admissao.AlunoId.HasValue)
        {
            TempData["Error"] = "Crie a matrícula antes de definir o status Matriculado.";
            return RedirecionarLocal(returnUrl, id);
        }

        var atualizado = await admissaoService.AtualizarProcessoAsync(
            id,
            status,
            contratoAssinado,
            pagamentoInicialConfirmado,
            integracaoConcluida,
            checklistObservacoes,
            cancellationToken);

        TempData[atualizado ? "Success" : "Error"] = atualizado
            ? "Processo de admissão atualizado."
            : "Não foi possível atualizar o processo de admissão.";

        return RedirecionarLocal(returnUrl, id);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdmissoesCreate)]
    public async Task<IActionResult> CriarMatricula(int id, AdmissaoMatriculaViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Detalhes da Admissão";
            var vmErro = await ConstruirDetalhesViewModelAsync(id, model, cancellationToken);

            if (vmErro is null)
            {
                return NotFound();
            }

            return View("Details", vmErro);
        }

        var input = new AdmissaoMatriculaInput
        {
            CPF = model.CPF,
            RG = model.RG,
            DataNascimento = model.DataNascimento,
            Endereco = model.Endereco,
            Celular = model.Celular,
            Email = model.Email,
            ContatoEmergencia = model.ContatoEmergencia,
            TurmaId = model.TurmaId,
            ObservacoesAluno = model.ObservacoesAluno
        };

        var resultado = await admissaoService.CriarMatriculaAsync(id, input, cancellationToken);

        if (!resultado.Sucesso)
        {
            ModelState.AddModelError(string.Empty, resultado.Erro ?? "Não foi possível criar a matrícula.");
            ViewData["Title"] = "Detalhes da Admissão";

            var vmErro = await ConstruirDetalhesViewModelAsync(id, model, cancellationToken);
            if (vmErro is null)
            {
                return NotFound();
            }

            return View("Details", vmErro);
        }

        TempData["Success"] = "Matrícula criada com sucesso e vinculada à admissão.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<AdmissaoDetalhesViewModel?> ConstruirDetalhesViewModelAsync(
        int id,
        AdmissaoMatriculaViewModel matriculaModel,
        CancellationToken cancellationToken)
    {
        var admissao = await admissaoService.ObterDetalhesAsync(id, cancellationToken);
        if (admissao is null)
        {
            return null;
        }

        var turmas = await admissaoService.ListarTurmasAsync(cancellationToken);

        return new AdmissaoDetalhesViewModel
        {
            Id = admissao.Id,
            NomeInteressado = admissao.NomeInteressado,
            DataAulaExperimental = admissao.DataAulaExperimental,
            DataMatricula = admissao.DataMatricula,
            Status = admissao.Status,
            ContratoAssinado = admissao.ContratoAssinado,
            PagamentoInicialConfirmado = admissao.PagamentoInicialConfirmado,
            IntegracaoConcluida = admissao.IntegracaoConcluida,
            ChecklistObservacoes = admissao.ChecklistObservacoes,
            AlunoId = admissao.AlunoId,
            AlunoNome = admissao.Aluno?.NomeCompleto,
            Matricula = matriculaModel,
            Turmas = turmas
                .Select(x => new AdmissaoTurmaOpcaoViewModel
                {
                    Id = x.Id,
                    Nome = x.Nome
                })
                .ToList()
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
