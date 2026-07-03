using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[AllowAnonymous]
[Route("blog")]
public class BlogController(IBlogService blogService) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] BlogPublicFilterViewModel filtro,
        CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Blog | IKKON SPTD";
        ViewData["Description"] = "Conteudos, novidades e bastidores do IKKON SPTD, escola de taiko em Sao Paulo.";
        ViewData["PublicSection"] = "blog";

        var viewModel = await blogService.ListarPublicoAsync(filtro, cancellationToken);
        return View(viewModel);
    }
}
