using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Admin)]
public class PainelAdminController(
    IAdminPainelService adminPainelService,
    IConfiguracaoService configuracaoService,
    ICurrentUserService currentUserService) : Controller
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminVisualizarDados)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Administração";
        var vm = await adminPainelService.ObterPainelAsync(cancellationToken);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarUsuarios)]
    public async Task<IActionResult> Usuarios(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        bool incluirExcluidos = false,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        ViewData["Title"] = "Gestao de Usuarios";
        var vm = await adminPainelService.ListarUsuariosAsync(
            busca,
            tipo,
            ativo,
            incluirExcluidos,
            pagina,
            tamanhoPagina,
            cancellationToken);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarUsuarios)]
    public async Task<IActionResult> NovoUsuario(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo Usuário";

        var model = new AdminUsuarioFormViewModel
        {
            Ativo = true,
            TipoAcesso = TipoAcessoEnum.Funcionario
        };

        await PopularRolesNoFormularioUsuarioAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarUsuarios)]
    public async Task<IActionResult> NovoUsuario(AdminUsuarioFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo Usuário";

        if (!ModelState.IsValid)
        {
            await PopularRolesNoFormularioUsuarioAsync(model, cancellationToken);
            return View(model);
        }

        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.CriarUsuarioAsync(
            model,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopularRolesNoFormularioUsuarioAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarUsuarios)]
    public async Task<IActionResult> EditarUsuario(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar Usuário";
        var model = await adminPainelService.ObterUsuarioParaEdicaoAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        await PopularRolesNoFormularioUsuarioAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarUsuarios)]
    public async Task<IActionResult> EditarUsuario(int id, AdminUsuarioFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar Usuário";

        if (id != model.Id)
        {
            return BadRequest();
        }

        model.SenhaInicial = null;

        if (!ModelState.IsValid)
        {
            await PopularRolesNoFormularioUsuarioAsync(model, cancellationToken);
            return View(model);
        }

        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.AtualizarUsuarioAsync(
            id,
            model,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopularRolesNoFormularioUsuarioAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarUsuarios)]
    public async Task<IActionResult> AlterarStatusUsuario(int id, bool ativo, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.AlterarStatusUsuarioAsync(
            id,
            ativo,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarUsuarios)]
    public async Task<IActionResult> ExcluirUsuario(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.ExcluirUsuarioAsync(
            id,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Usuarios));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminEditarPermissoes)]
    public async Task<IActionResult> Acessos(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Permissões e Acessos";
        var vm = await adminPainelService.ObterAcessosAsync(id, cancellationToken);
        if (vm is null)
        {
            return NotFound();
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminEditarPermissoes)]
    public async Task<IActionResult> Acessos(AdminAcessosUpdateRequest request, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Permissões e Acessos";

        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.AtualizarAcessosAsync(
            request,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        if (!result.Success)
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Acessos), new { id = request.UsuarioId });
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Acessos), new { id = request.UsuarioId });
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarCargos)]
    public async Task<IActionResult> Cargos(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        ViewData["Title"] = "Cargos";
        var vm = await adminPainelService.ListarRolesAsync(
            busca,
            tipo,
            ativo,
            pagina,
            tamanhoPagina,
            cancellationToken);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarCargos)]
    [Authorize(Policy = AuthorizationPolicies.AdminEditarPermissoes)]
    public async Task<IActionResult> NovoCargo(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo Cargo";
        var model = await adminPainelService.ObterRoleParaCriacaoAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarCargos)]
    [Authorize(Policy = AuthorizationPolicies.AdminEditarPermissoes)]
    public async Task<IActionResult> NovoCargo(AdminRoleFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Novo Cargo";

        if (!ModelState.IsValid)
        {
            await PopularPermissoesNoFormularioCargoAsync(model, cancellationToken);
            return View(model);
        }

        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.CriarRoleAsync(
            model,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopularPermissoesNoFormularioCargoAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Cargos));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarCargos)]
    [Authorize(Policy = AuthorizationPolicies.AdminEditarPermissoes)]
    public async Task<IActionResult> EditarCargo(int id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar Cargo";
        var model = await adminPainelService.ObterRoleParaEdicaoAsync(id, cancellationToken);
        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarCargos)]
    [Authorize(Policy = AuthorizationPolicies.AdminEditarPermissoes)]
    public async Task<IActionResult> EditarCargo(int id, AdminRoleFormViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Editar Cargo";

        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopularPermissoesNoFormularioCargoAsync(model, cancellationToken);
            return View(model);
        }

        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.AtualizarRoleAsync(
            id,
            model,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            await PopularPermissoesNoFormularioCargoAsync(model, cancellationToken);
            return View(model);
        }

        TempData["Success"] = result.Message;
        return RedirectToAction(nameof(Cargos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarCargos)]
    public async Task<IActionResult> AlterarStatusCargo(int id, bool ativo, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.AlterarStatusRoleAsync(
            id,
            ativo,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Cargos));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarCargos)]
    public async Task<IActionResult> ExcluirCargo(int id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var usuarioResponsavelId))
        {
            return Challenge();
        }

        var result = await adminPainelService.ExcluirRoleAsync(
            id,
            usuarioResponsavelId,
            ObterIpRequisicao(),
            cancellationToken);

        TempData[result.Success ? "Success" : "Error"] = result.Message;
        return RedirectToAction(nameof(Cargos));
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarSistema)]
    public async Task<IActionResult> Logs(
        string? busca,
        int? usuarioResponsavelId,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        ViewData["Title"] = "Logs e Auditoria";
        var vm = await adminPainelService.ListarLogsAsync(
            busca,
            usuarioResponsavelId,
            pagina,
            tamanhoPagina,
            cancellationToken);
        return View(vm);
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarSistema)]
    public async Task<IActionResult> Sistema(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Configuracoes do Sistema";
        var vm = await configuracaoService.ObterPainelAsync(cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarSistema)]
    public async Task<IActionResult> Sistema(ConfiguracoesFormViewModel form, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Configuracoes do Sistema";

        if (!ModelState.IsValid)
        {
            var vmInvalido = await configuracaoService.ObterPainelAsync(cancellationToken);
            vmInvalido.Form = form;
            return View(vmInvalido);
        }

        await configuracaoService.SalvarAsync(form, cancellationToken);
        TempData["Success"] = "Configuracoes do sistema atualizadas com sucesso.";
        return RedirectToAction(nameof(Sistema));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = AuthorizationPolicies.AdminGerenciarSistema)]
    public async Task<IActionResult> RestaurarSistema(CancellationToken cancellationToken)
    {
        await configuracaoService.RestaurarPadraoAsync(cancellationToken);
        TempData["Success"] = "Configuracoes restauradas para os padroes.";
        return RedirectToAction(nameof(Sistema));
    }

    private async Task PopularRolesNoFormularioUsuarioAsync(AdminUsuarioFormViewModel model, CancellationToken cancellationToken)
    {
        model.RolesDisponiveis = await adminPainelService.ListarRolesAtivasAsync(null, cancellationToken);

        if (model.RoleId <= 0 && model.RolesDisponiveis.Count > 0)
        {
            var rolePadraoFuncionario = model.RolesDisponiveis.FirstOrDefault(x => x.Codigo == AppRoles.Funcionario);
            model.RoleId = rolePadraoFuncionario?.Id ?? model.RolesDisponiveis[0].Id;
        }

        var roleSelecionada = model.RolesDisponiveis.FirstOrDefault(x => x.Id == model.RoleId);
        if (roleSelecionada is not null)
        {
            model.TipoAcesso = roleSelecionada.TipoAcesso;
        }
    }

    private async Task PopularPermissoesNoFormularioCargoAsync(AdminRoleFormViewModel model, CancellationToken cancellationToken)
    {
        if (model.Id.HasValue)
        {
            var existente = await adminPainelService.ObterRoleParaEdicaoAsync(model.Id.Value, cancellationToken);
            if (existente is not null)
            {
                model.PermissoesDisponiveis = existente.PermissoesDisponiveis;
            }
        }
        else
        {
            var novo = await adminPainelService.ObterRoleParaCriacaoAsync(cancellationToken);
            model.PermissoesDisponiveis = novo.PermissoesDisponiveis;
        }

        var selecionadas = model.PermissoesSelecionadas.ToHashSet();
        foreach (var permissao in model.PermissoesDisponiveis)
        {
            permissao.Concedida = selecionadas.Contains(permissao.Id);
            permissao.HerdadaDaRole = false;
        }
    }

    private bool TryGetCurrentUserId(out int userId)
    {
        if (currentUserService.UserId is int currentUserId)
        {
            userId = currentUserId;
            return true;
        }

        userId = 0;
        return false;
    }

    private string? ObterIpRequisicao()
    {
        return currentUserService.RemoteIpAddress;
    }
}
