namespace Web.Ui.Services;

/// <summary>
/// Lightweight mirror of the authoritative Nowruz table used by
/// <c>Infrastructure.Services.PersianCalendarService</c>. Web.Ui does not
/// reference the other projects, so this copy is kept here for the small
/// "which hire-date scenario applies" decision in the employee form.
/// If the Nowruz table is extended there, extend it here too.
/// </summary>
public static class PersianCalendarLite
{
    private const int MinYear = 1399;
    private const int MaxYear = 1415;

    private static readonly Dictionary<int, (int Year, int Month, int Day)> NowruzByPersianYear = new()
    {
        [1399] = (2020, 3, 20),
        [1400] = (2021, 3, 21),
        [1401] = (2022, 3, 21),
        [1402] = (2023, 3, 21),
        [1403] = (2024, 3, 20),
        [1404] = (2025, 3, 20),
        [1405] = (2026, 3, 20),
        [1406] = (2027, 3, 21),
        [1407] = (2028, 3, 20),
        [1408] = (2029, 3, 20),
        [1409] = (2030, 3, 20),
        [1410] = (2031, 3, 20),
        [1411] = (2032, 3, 20),
        [1412] = (2033, 3, 20),
        [1413] = (2034, 3, 20),
        [1414] = (2035, 3, 20),
        [1415] = (2036, 3, 20),
    };

    public static (int Year, int Month) GetPersianYearMonth(DateOnly date)
    {
        var persianYear = GetPersianYear(date);
        var nowruz = NowruzByPersianYear[persianYear];
        var nowruzDate = new DateOnly(nowruz.Year, nowruz.Month, nowruz.Day);

        var offsetDays = (date.ToDateTime(TimeOnly.MinValue) - nowruzDate.ToDateTime(TimeOnly.MinValue)).Days;

        var persianMonth = offsetDays < 186 ? 1 + offsetDays / 31 : 7 + (offsetDays - 186) / 30;

        return (persianYear, persianMonth);
    }

    public static int GetPersianYear(DateOnly date)
    {
        for (var year = MaxYear; year >= MinYear; year--)
        {
            var nowruz = NowruzByPersianYear[year];
            var nowruzDate = new DateOnly(nowruz.Year, nowruz.Month, nowruz.Day);

            if (date >= nowruzDate)
                return year;
        }

        throw new ArgumentOutOfRangeException(nameof(date), "تاریخ خارج از بازه پشتیبانی تقویم است.");
    }
}
