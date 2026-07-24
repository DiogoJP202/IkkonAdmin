using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.TurmasView)]
public class TurmasController(ITurmaService turmaService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, bool? ativa, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Turmas";

        var turmas = await turmaService.ListarAsync(busca, ativa, cancellationToken);

        var vm = new TurmaIndexViewModel
        {
            Busca = busca,
            Ativa = ativa,
            Turmas = turmas
                .Select(x => new TurmaListItemViewModel
                {
                    Id = x.Id,
                    Nome = x.Nome,
                    Modalidade = x.Modalidade,
                    Horario = x.Horario,
                    Ativa = x.Ativa,
                    QuantidadeAlunos = x.AlunoTurmas.Select(t => t.AlunoId).Distinct().Count()
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.TurmasCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Nova Turma";

        var vm = new TurmaFormViewModel
        {
            Ativa = true
        };

        await PopularAlunosAsync(vm, null, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.TurmasCreate)]
    public async Task<IActionResult> Create(TurmaFormViewModel model, CancellationToken cancellationToken)
    {
        model.AlunosIds ??= [];

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Nova Turma";
            await PopularAlunosAsync(model, null, cancellationToken);
            return View(model);
        }

        var turma = new Turma
        {
            Nome = model.Nome,
            Modalidade = model.Modalidade,
            Horario = model.Horario,
            Ativa = model.Ativa,
            Observacoes = model.Observacoes
        };

        var result = await turmaService.CriarAsync(turma, model.AlunosIds, cancellationToken);
        if (!result.Success)
        {
            result.AddToModelState(ModelState);
            ViewData["Title"] = "Nova Turma";
            await PopularAlunosAsync(model, null, cancellationToken);
            return View(model);
        }

        result.AddToTempData(TempData);
        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.TurmasEdit)]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar Turma";

        var turma = await turmaService.ObterComAlunosAsync(id, cancellationToken);
        if (turma is null)
        {
            return NotFound();
        }

        var vm = new TurmaFormViewModel
        {
            Id = turma.Id,
            Nome = turma.Nome,
            Modalidade = turma.Modalidade,
            Horario = turma.Horario,
            Ativa = turma.Ativa,
            Observacoes = turma.Observacoes,
            AlunosIds = turma.AlunoTurmas
                .Select(x => x.AlunoId)
                .Distinct()
                .ToList()
        };

        await PopularAlunosAsync(vm, turma.Id, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.TurmasEdit)]
    public async Task<IActionResult> Edit(int id, TurmaFormViewModel model, CancellationToken cancellationToken)
    {
        model.AlunosIds ??= [];

        if (model.Id != id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Editar Turma";
            await PopularAlunosAsync(model, id, cancellationToken);
            return View(model);
        }

        var turmaAtualizada = new Turma
        {
            Nome = model.Nome,
            Modalidade = model.Modalidade,
            Horario = model.Horario,
            Ativa = model.Ativa,
            Observacoes = model.Observacoes
        };

        var result = await turmaService.AtualizarAsync(id, turmaAtualizada, model.AlunosIds, cancellationToken);
        if (result.Status == OperationResultStatus.NotFound)
        {
            return NotFound();
        }

        if (!result.Success)
        {
            result.AddToModelState(ModelState);
            ViewData["Title"] = "Editar Turma";
            await PopularAlunosAsync(model, id, cancellationToken);
            return View(model);
        }

        result.AddToTempData(TempData);
        return RedirectToAction(nameof(Index));
    }

    private async Task PopularAlunosAsync(TurmaFormViewModel model, int? turmaIdAtual, CancellationToken cancellationToken)
    {
        var alunos = await turmaService.ListarAlunosVinculaveisAsync(turmaIdAtual, cancellationToken);
        model.AlunosDisponiveis = alunos
            .Select(x => new TurmaAlunoOpcaoViewModel
            {
                Id = x.Id,
                NomeCompleto = x.NomeCompleto,
                Status = x.Status,
                TurmaAtual = x.AlunoTurmas.Count == 0
                    ? null
                    : string.Join(", ", x.AlunoTurmas
                        .Select(t => t.Turma.Nome)
                        .Distinct()
                        .OrderBy(nome => nome))
            })
            .ToList();
    }
}
