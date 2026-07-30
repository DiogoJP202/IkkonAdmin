using System.Globalization;

namespace IkkonAdmin.Web.Helpers;

public static class PublicViewFormatter
{
    public static string FormatBlogDate(DateTime utcDate)
    {
        try
        {
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(utcDate, DateTimeKind.Utc),
                GetSaoPauloTimeZone());

            return localDate.ToString("dd MMM yyyy", CultureInfo.CurrentUICulture);
        }
        catch (TimeZoneNotFoundException)
        {
            return utcDate.ToString("dd MMM yyyy", CultureInfo.CurrentUICulture);
        }
        catch (InvalidTimeZoneException)
        {
            return utcDate.ToString("dd MMM yyyy", CultureInfo.CurrentUICulture);
        }
    }

    public static string BuildBlogUrl(string slug) => $"/blog/{slug}";

    private static TimeZoneInfo GetSaoPauloTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }
}
