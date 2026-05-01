using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

public class InstitucionalController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "IKKON SPTD | Taiko em São Paulo";
        ViewData["Description"] = "IKKON SPTD em São Paulo: escola de taiko, fue e teoria musical, além de grupo artístico para eventos e apresentações.";
        ViewData["PublicSection"] = "home";
        return View();
    }

    [HttpGet]
    public IActionResult Escola()
    {
        ViewData["Title"] = "Escola de Taiko em São Paulo | IKKON SPTD";
        ViewData["Description"] = "Aulas de taiko, fue e teoria musical em São Paulo para iniciantes e alunos em evolução.";
        ViewData["PublicSection"] = "escola";
        return View();
    }

    [HttpGet]
    public IActionResult Eventos()
    {
        ViewData["Title"] = "Apresentações de Taiko para Eventos | IKKON SPTD";
        ViewData["Description"] = "Contrate apresentações de taiko para eventos culturais, corporativos, festivais e ações especiais.";
        ViewData["PublicSection"] = "eventos";
        return View();
    }
}
