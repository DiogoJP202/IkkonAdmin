using IkkonAdmin.Web.Helpers;
using Microsoft.AspNetCore.Localization;

namespace IkkonAdmin.Web.Infrastructure.Localization;

public sealed class PublicPathRequestCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(
        HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var languageSegment = httpContext.Request.Path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        var result = PublicSiteLocales.TryFromSegment(languageSegment, out var locale)
            ? new ProviderCultureResult(locale.Culture, locale.Culture)
            : null;

        return Task.FromResult(result);
    }
}
