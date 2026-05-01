using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.Aluno)]
public class AlunoAreaController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Area do Aluno";
        return View();
    }
}
