namespace IkkonAdmin.Web.Services;

public sealed class BlogDateTimeService : IBlogDateTimeService
{
    public DateTime ConvertSaoPauloLocalToUtc(DateTime localDateTime)
    {
        var timeZone = GetSaoPauloTimeZone();
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
    }

    public DateTime? ConvertUtcToSaoPauloLocal(DateTime? utcDateTime)
    {
        if (!utcDateTime.HasValue)
        {
            return null;
        }

        var timeZone = GetSaoPauloTimeZone();
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime.Value, DateTimeKind.Utc), timeZone);
    }

    public DateTime ConvertSaoPauloDateOnlyToUtcStart(DateOnly date)
    {
        return ConvertSaoPauloLocalToUtc(date.ToDateTime(TimeOnly.MinValue));
    }

    public DateTime ConvertSaoPauloDateOnlyToUtcEndExclusive(DateOnly date)
    {
        return ConvertSaoPauloLocalToUtc(date.AddDays(1).ToDateTime(TimeOnly.MinValue));
    }

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
