using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.AlunosView)]
public class AlunosController(
    IAlunoQueryService alunoQueryService,
    IAlunoService alunoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? busca,
        StatusAlunoEnum? status,
        int? turmaId,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        ViewData["Title"] = "Alunos";
        tamanhoPagina = NormalizarTamanhoPagina(tamanhoPagina);

        var resultado = await alunoQueryService.ListarAsync(busca, status, turmaId, pagina, tamanhoPagina, cancellationToken);
        var turmas = await alunoQueryService.ListarTurmasAsync(cancellationToken);

        var vm = new AlunoIndexViewModel
        {
            Busca = busca,
            Status = status,
            TurmaId = turmaId,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = resultado.TotalRegistros,
            Turmas = turmas
                .Select(x => new TurmaFiltroViewModel
                {
                    Id = x.Id,
                    Nome = x.Nome
                })
                .ToList(),
            Alunos = resultado.Itens
                .Select(x => new AlunoListItemViewModel
                {
                    Id = x.Id,
                    NomeCompleto = x.NomeCompleto,
                    CPF = x.CPF,
                    Celular = x.Celular,
                    Turma = x.Turma?.Nome,
                    Status = x.Status,
                    DataEntrada = x.DataEntrada
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AlunosCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo Aluno";

        var model = new AlunoFormViewModel
        {
            DataEntrada = DateOnly.FromDateTime(DateTime.Today)
        };

        await PopularTurmasAsync(model.TurmaId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AlunosCreate)]
    public async Task<IActionResult> Create(AlunoFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Novo Aluno";
            await PopularTurmasAsync(model.TurmaId, cancellationToken);
            return View(model);
        }

        var aluno = new Aluno
        {
            NomeCompleto = model.NomeCompleto,
            CPF = model.CPF,
            RG = model.RG,
            DataNascimento = model.DataNascimento,
            Endereco = model.Endereco,
            Celular = model.Celular,
            Email = model.Email,
            ContatoEmergencia = model.ContatoEmergencia,
            DataEntrada = model.DataEntrada,
            TurmaId = model.TurmaId,
            Status = model.Status,
            Observacoes = model.Observacoes
        };

        var result = await alunoService.CriarAsync(aluno, cancellationToken);
        if (!result.Success)
        {
            result.AddToModelState(ModelState);
            ViewData["Title"] = "Novo Aluno";
            await PopularTurmasAsync(model.TurmaId, cancellationToken);
            return View(model);
        }

        result.AddToTempData(TempData);
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AlunosEdit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar Aluno";

        var aluno = await alunoQueryService.ObterDetalhesAsync(id, cancellationToken);
        if (aluno is null)
        {
            return NotFound();
        }

        var model = new AlunoFormViewModel
        {
            Id = aluno.Id,
            NomeCompleto = aluno.NomeCompleto,
            CPF = aluno.CPF,
            RG = aluno.RG,
            DataNascimento = aluno.DataNascimento,
            Endereco = aluno.Endereco,
            Celular = aluno.Celular,
            Email = aluno.Email,
            ContatoEmergencia = aluno.ContatoEmergencia,
            DataEntrada = aluno.DataEntrada,
            TurmaId = aluno.TurmaId,
            Status = aluno.Status,
            Observacoes = aluno.Observacoes
        };

        await PopularTurmasAsync(model.TurmaId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AlunosEdit)]
    public async Task<IActionResult> Edit(int id, AlunoFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Editar Aluno";
            await PopularTurmasAsync(model.TurmaId, cancellationToken);
            return View(model);
        }

        var aluno = new Aluno
        {
            NomeCompleto = model.NomeCompleto,
            CPF = model.CPF,
            RG = model.RG,
            DataNascimento = model.DataNascimento,
            Endereco = model.Endereco,
            Celular = model.Celular,
            Email = model.Email,
            ContatoEmergencia = model.ContatoEmergencia,
            DataEntrada = model.DataEntrada,
            TurmaId = model.TurmaId,
            Status = model.Status,
            Observacoes = model.Observacoes
        };

        var result = await alunoService.AtualizarAsync(id, aluno, cancellationToken);
        if (result.Status == OperationResultStatus.NotFound)
        {
            return NotFound();
        }

        if (!result.Success)
        {
            result.AddToModelState(ModelState);
            ViewData["Title"] = "Editar Aluno";
            await PopularTurmasAsync(model.TurmaId, cancellationToken);
            return View(model);
        }

        result.AddToTempData(TempData);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Detalhes do Aluno";

        var aluno = await alunoQueryService.ObterDetalhesAsync(id, cancellationToken);
        if (aluno is null)
        {
            return NotFound();
        }

        var mensalidades = aluno.Mensalidades
            .OrderByDescending(x => x.Competencia)
            .Take(18)
            .Select(x => new AlunoMensalidadeViewModel
            {
                Id = x.Id,
                Competencia = x.Competencia,
                DataVencimento = x.DataVencimento,
                ValorFinal = x.ValorFinal,
                Status = x.Status,
                DataPagamento = x.DataPagamento
            })
            .ToList();

        var historico = aluno.Historicos
            .OrderByDescending(x => x.DataEvento)
            .Take(25)
            .Select(x => new AlunoHistoricoItemViewModel
            {
                DataEvento = x.DataEvento,
                TipoEvento = x.TipoEvento,
                Descricao = x.Descricao
            })
            .ToList();

        var vm = new AlunoDetalhesViewModel
        {
            Id = aluno.Id,
            NomeCompleto = aluno.NomeCompleto,
            CPF = aluno.CPF,
            RG = aluno.RG,
            DataNascimento = aluno.DataNascimento,
            Endereco = aluno.Endereco,
            Celular = aluno.Celular,
            Email = aluno.Email,
            ContatoEmergencia = aluno.ContatoEmergencia,
            DataEntrada = aluno.DataEntrada,
            Turma = aluno.Turma?.Nome,
            Status = aluno.Status,
            Observacoes = aluno.Observacoes,
            TotalPago = aluno.Pagamentos.Sum(x => x.ValorPago),
            TotalEmAberto = aluno.Mensalidades
                .Where(x => x.Status == StatusMensalidadeEnum.Pendente || x.Status == StatusMensalidadeEnum.Atrasado)
                .Sum(x => x.ValorFinal),
            MensalidadesAtrasadas = aluno.Mensalidades.Count(x => x.Status == StatusMensalidadeEnum.Atrasado),
            Mensalidades = mensalidades,
            Historico = historico
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AlunosEdit)]
    public async Task<IActionResult> AlterarStatus(int id, StatusAlunoEnum status, string? returnUrl, CancellationToken cancellationToken)
    {
        var result = await alunoService.AlterarStatusAsync(id, status, cancellationToken);
        if (result.Status == OperationResultStatus.NotFound)
        {
            return NotFound();
        }

        result.AddToTempData(TempData);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopularTurmasAsync(int? turmaSelecionada, CancellationToken cancellationToken)
    {
        var turmas = await alunoQueryService.ListarTurmasAsync(cancellationToken);
        ViewBag.Turmas = new SelectList(turmas, nameof(Turma.Id), nameof(Turma.Nome), turmaSelecionada);
    }

    private static int NormalizarTamanhoPagina(int tamanhoPagina)
    {
        return tamanhoPagina is 10 or 20 or 30 ? tamanhoPagina : 20;
    }
}
