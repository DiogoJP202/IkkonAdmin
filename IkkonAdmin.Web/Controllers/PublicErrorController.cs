using System.Globalization;
using IkkonAdmin.Web.Helpers;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Controllers;

[AllowAnonymous]
public sealed class PublicErrorController(IViewTextService i18n) : Controller
{
    [HttpGet("/erro/{statusCode:int}")]
    public IActionResult StatusCodePage(int statusCode)
    {
        if (statusCode != StatusCodes.Status404NotFound)
        {
            return StatusCode(statusCode);
        }

        var originalRequest = HttpContext.Features
            .Get<IStatusCodeReExecuteFeature>()?
            .OriginalPath;
        var originalLanguageSegment = originalRequest?
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();
        if (PublicSiteLocales.TryFromSegment(originalLanguageSegment, out var originalLocale))
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(originalLocale.Culture);
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(originalLocale.Culture);
        }

        Response.StatusCode = StatusCodes.Status404NotFound;
        ViewData["Title"] = i18n[
            "Página não encontrada | IKKON SPTD",
            "Page not found | IKKON SPTD",
            "ページが見つかりません | IKKON SPTD"];
        ViewData["Description"] = i18n[
            "A página solicitada não foi encontrada no site do IKKON SPTD.",
            "The requested page was not found on the IKKON SPTD website.",
            "IKKON SPTDのサイトで、ご指定のページが見つかりませんでした。"];
        ViewData["Robots"] = "noindex,follow";
        ViewData["SuppressCanonicalAndAlternates"] = true;
        ViewData["PublicSection"] = string.Empty;
        ViewData["OriginalRequestPath"] = originalRequest;

        return View("NotFound");
    }
}
