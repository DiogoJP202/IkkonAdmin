using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

public class InstitucionalController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Ikkon SPTD | Escola de Taiko";
        return View();
    }
}
