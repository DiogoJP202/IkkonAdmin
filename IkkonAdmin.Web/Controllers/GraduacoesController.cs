using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.GraduacoesView)]
public class GraduacoesController(IGraduacaoService graduacaoService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? busca, bool? somenteAprovados, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Graduações";

        var graduacoes = await graduacaoService.ListarAsync(busca, somenteAprovados, cancellationToken);
        var exames = await graduacaoService.ListarExamesAsync(cancellationToken);
        var alunosAptos = await graduacaoService.ListarAlunosAptosAsync(cancellationToken);
        var resumoNivelAtual = await ObterResumoNivelAtualAsync(cancellationToken);

        var vm = new GraduacaoIndexViewModel
        {
            Busca = busca,
            ApenasAprovados = somenteAprovados,
            Graduacoes = graduacoes
                .Select(x => new GraduacaoListItemViewModel
                {
                    Id = x.Id,
                    AlunoId = x.AlunoId,
                    AlunoNome = x.Aluno?.NomeCompleto ?? $"Aluno #{x.AlunoId}",
                    TurmaNome = x.Aluno?.Turma?.Nome,
                    DataResultado = x.DataResultado,
                    ResultadoAprovado = x.ResultadoAprovado,
                    NivelAnterior = x.NivelAnterior,
                    NivelNovo = x.NivelNovo,
                    CertificadoEmitido = x.CertificadoEmitido,
                    OmamoriAtualizado = x.OmamoriAtualizado,
                    ExameGraduacaoId = x.ExameGraduacaoId,
                    DataExame = x.ExameGraduacao?.DataExame
                })
                .ToList(),
            Exames = exames
                .Take(12)
                .Select(x => new ExameGraduacaoListItemViewModel
                {
                    Id = x.Id,
                    DataExame = x.DataExame,
                    Local = x.Local,
                    NivelPretendido = x.NivelPretendido,
                    ResultadosRegistrados = x.Graduacoes.Count
                })
                .ToList(),
            AlunosAptos = alunosAptos
                .Take(20)
                .Select(x =>
                {
                    var possuiResumo = resumoNivelAtual.TryGetValue(x.Id, out var resumo);
                    return new GraduacaoAlunoAptoViewModel
                    {
                        AlunoId = x.Id,
                        AlunoNome = x.NomeCompleto,
                        TurmaNome = x.Turma?.Nome,
                        NivelAtual = possuiResumo ? resumo!.NivelAtual : NivelGraduacaoEnum.Iniciante,
                        UltimoResultado = possuiResumo ? resumo!.DataUltimoResultado : null
                    };
                })
                .ToList(),
            NovoExame = new ExameGraduacaoCreateViewModel()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.GraduacoesCreate)]
    public async Task<IActionResult> CriarExame(ExameGraduacaoCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dados inválidos para criar exame.";
            return RedirectToAction(nameof(Index));
        }

        var exame = new ExameGraduacao
        {
            DataExame = model.DataExame,
            Local = model.Local,
            NivelPretendido = model.NivelPretendido,
            Observacoes = model.Observacoes
        };

        var result = await graduacaoService.CriarExameAsync(exame, cancellationToken);
        result.AddToTempData(TempData);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.GraduacoesCreate)]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Registrar Resultado";

        var model = new GraduacaoViewModel();
        await PopularDadosFormularioAsync(model, cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.GraduacoesCreate)]
    public async Task<IActionResult> Create(GraduacaoViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Registrar Resultado";
            await PopularDadosFormularioAsync(model, cancellationToken);
            return View(model);
        }

        var input = new GraduacaoRegistroInput
        {
            AlunoId = model.AlunoId,
            ExameGraduacaoId = model.ExameGraduacaoId,
            DataExameNovo = model.ExameGraduacaoId.HasValue ? null : model.DataExameNovo,
            LocalExameNovo = model.ExameGraduacaoId.HasValue ? null : model.LocalExameNovo,
            NivelPretendidoExameNovo = model.NivelPretendidoExameNovo,
            DataResultado = model.DataResultado,
            ResultadoAprovado = model.ResultadoAprovado,
            NivelNovo = model.NivelNovo,
            CertificadoEmitido = model.CertificadoEmitido,
            OmamoriAtualizado = model.OmamoriAtualizado,
            Observacoes = model.Observacoes
        };

        var resultado = await graduacaoService.RegistrarResultadoAsync(input, cancellationToken);

        if (!resultado.Success || resultado.Value is null)
        {
            resultado.AddToModelState(ModelState);
            ViewData["Title"] = "Registrar Resultado";
            await PopularDadosFormularioAsync(model, cancellationToken);
            return View(model);
        }

        resultado.AddToTempData(TempData);
        return RedirectToAction(nameof(Details), new { id = resultado.Value.GraduacaoId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Detalhes da Graduação";

        var graduacao = await graduacaoService.ObterDetalhesAsync(id, cancellationToken);
        if (graduacao is null)
        {
            return NotFound();
        }

        var historicoAluno = await graduacaoService.ListarHistoricoAlunoAsync(graduacao.AlunoId, cancellationToken);

        var vm = new GraduacaoDetalhesViewModel
        {
            Id = graduacao.Id,
            AlunoId = graduacao.AlunoId,
            AlunoNome = graduacao.Aluno?.NomeCompleto ?? $"Aluno #{graduacao.AlunoId}",
            TurmaNome = graduacao.Aluno?.Turma?.Nome,
            DataResultado = graduacao.DataResultado,
            ResultadoAprovado = graduacao.ResultadoAprovado,
            NivelAnterior = graduacao.NivelAnterior,
            NivelNovo = graduacao.NivelNovo,
            CertificadoEmitido = graduacao.CertificadoEmitido,
            OmamoriAtualizado = graduacao.OmamoriAtualizado,
            Observacoes = graduacao.Observacoes,
            ExameGraduacaoId = graduacao.ExameGraduacaoId,
            DataExame = graduacao.ExameGraduacao?.DataExame,
            LocalExame = graduacao.ExameGraduacao?.Local,
            NivelPretendidoExame = graduacao.ExameGraduacao?.NivelPretendido,
            HistoricoAluno = historicoAluno
                .Select(x => new GraduacaoDetalhesHistoricoViewModel
                {
                    Id = x.Id,
                    DataResultado = x.DataResultado,
                    ResultadoAprovado = x.ResultadoAprovado,
                    NivelAnterior = x.NivelAnterior,
                    NivelNovo = x.NivelNovo
                })
                .ToList()
        };

        return View(vm);
    }

    private async Task PopularDadosFormularioAsync(GraduacaoViewModel model, CancellationToken cancellationToken)
    {
        var alunos = await graduacaoService.ListarAlunosAptosAsync(cancellationToken);
        var exames = await graduacaoService.ListarExamesAsync(cancellationToken);
        var resumoNivelAtual = await ObterResumoNivelAtualAsync(cancellationToken);

        model.AlunosDisponiveis = alunos
            .Select(x =>
            {
                var nivelAtual = resumoNivelAtual.TryGetValue(x.Id, out var resumo)
                    ? resumo!.NivelAtual
                    : NivelGraduacaoEnum.Iniciante;

                return new GraduacaoAlunoOpcaoViewModel
                {
                    Id = x.Id,
                    Nome = x.NomeCompleto,
                    TurmaNome = x.Turma?.Nome,
                    NivelAtual = nivelAtual
                };
            })
            .ToList();

        model.ExamesDisponiveis = exames
            .Select(x => new GraduacaoExameOpcaoViewModel
            {
                Id = x.Id,
                DataExame = x.DataExame,
                Local = x.Local,
                NivelPretendido = x.NivelPretendido
            })
            .ToList();
    }

    private async Task<Dictionary<int, ResumoNivelAtual>> ObterResumoNivelAtualAsync(CancellationToken cancellationToken)
    {
        var graduacoesAprovadas = await graduacaoService.ListarAsync(
            somenteAprovados: true,
            cancellationToken: cancellationToken);

        return graduacoesAprovadas
            .GroupBy(x => x.AlunoId)
            .ToDictionary(
                x => x.Key,
                x =>
                {
                    var ultima = x
                        .OrderByDescending(g => g.DataResultado)
                        .ThenByDescending(g => g.Id)
                        .First();

                    return new ResumoNivelAtual
                    {
                        NivelAtual = ultima.NivelNovo ?? ultima.NivelAnterior,
                        DataUltimoResultado = ultima.DataResultado
                    };
                });
    }

    private sealed class ResumoNivelAtual
    {
        public NivelGraduacaoEnum NivelAtual { get; init; }
        public DateOnly DataUltimoResultado { get; init; }
    }
}
