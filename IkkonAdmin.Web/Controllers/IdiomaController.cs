using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[Route("idioma")]
public class IdiomaController : Controller
{
    private static readonly HashSet<string> CulturasSuportadas = new(StringComparer.OrdinalIgnoreCase)
    {
        "pt-BR",
        "en-US",
        "ja-JP"
    };

    [HttpGet("alterar")]
    public IActionResult Alterar(string culture, string? returnUrl = null)
    {
        var cultura = CulturasSuportadas.Contains(culture) ? culture : "pt-BR";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(cultura)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return LocalRedirect("/");
    }
}
