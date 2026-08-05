using System.Diagnostics;
using IkkonAdmin.Web.Models;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.DashboardView)]
public class HomeController(IDashboardQueryService dashboardQueryService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        int? anoReferencia,
        int? mesReferencia,
        int? turmaId,
        CancellationToken cancellationToken)
    {
        var dashboard = await dashboardQueryService.ObterDashboardAsync(
            anoReferencia,
            mesReferencia,
            turmaId,
            cancellationToken);

        return View(dashboard);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
