namespace IkkonAdmin.Web.Infrastructure.Time;

public static class SaoPauloTime
{
    public static TimeZoneInfo TimeZone { get; } = ResolveTimeZone();

    public static DateTime FromUtc(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc),
            TimeZone);
    }

    public static DateOnly Today(DateTime utcNow)
    {
        return DateOnly.FromDateTime(FromUtc(utcNow));
    }

    private static TimeZoneInfo ResolveTimeZone()
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
