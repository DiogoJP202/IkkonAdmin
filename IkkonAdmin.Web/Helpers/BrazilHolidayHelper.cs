using System.Collections.Concurrent;

namespace IkkonAdmin.Web.Helpers;

public static class BrazilHolidayHelper
{
    private static readonly ConcurrentDictionary<int, IReadOnlyDictionary<DateOnly, string>> Cache = new();

    public static string? GetHolidayName(DateOnly date)
    {
        var holidays = Cache.GetOrAdd(date.Year, BuildHolidayMap);
        return holidays.TryGetValue(date, out var holidayName) ? holidayName : null;
    }

    private static IReadOnlyDictionary<DateOnly, string> BuildHolidayMap(int year)
    {
        var easterSunday = GetEasterSunday(year);

        return new Dictionary<DateOnly, string>
        {
            [new DateOnly(year, 1, 1)] = "Confraternizacao Universal",
            [new DateOnly(year, 1, 25)] = "Aniversario de Sao Paulo",
            [easterSunday.AddDays(-48)] = "Carnaval",
            [easterSunday.AddDays(-47)] = "Carnaval",
            [easterSunday.AddDays(-2)] = "Sexta-feira Santa",
            [new DateOnly(year, 4, 21)] = "Tiradentes",
            [new DateOnly(year, 5, 1)] = "Dia do Trabalho",
            [easterSunday.AddDays(60)] = "Corpus Christi",
            [new DateOnly(year, 7, 9)] = "Revolucao Constitucionalista",
            [new DateOnly(year, 9, 7)] = "Independencia do Brasil",
            [new DateOnly(year, 10, 12)] = "Nossa Senhora Aparecida",
            [new DateOnly(year, 11, 2)] = "Finados",
            [new DateOnly(year, 11, 15)] = "Proclamacao da Republica",
            [new DateOnly(year, 11, 20)] = "Dia da Consciencia Negra",
            [new DateOnly(year, 12, 25)] = "Natal"
        };
    }

    private static DateOnly GetEasterSunday(int year)
    {
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = ((h + l - 7 * m + 114) % 31) + 1;

        return new DateOnly(year, month, day);
    }
}
