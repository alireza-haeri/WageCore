namespace Infrastructure.Services;

public class PersianCalendarService : IPersianCalendarService
{
    // The official Iranian calendar places Nowruz on the day of the March equinox
    // (Tehran local time), as announced for each year.
    // System.Globalization.PersianCalendar instead uses the 2820-year arithmetic
    // cycle, which drifts from the official calendar in recent decades (for example
    // it places Nowruz 1405 on 2026-03-21, while the official Nowruz 1405 is
    // 2026-03-20). So the Gregorian Nowruz dates of the supported years are kept
    // explicitly and every other value is derived from them.
    //
    // Sources: the published March equinox moments (2020-2036, Tehran local time)
    // and the official Iranian calendar announcements for each year.
    private static readonly Dictionary<int, DateOnly> NowruzByPersianYear = new()
    {
        [1399] = new(2020, 3, 20),
        [1400] = new(2021, 3, 21),
        [1401] = new(2022, 3, 21),
        [1402] = new(2023, 3, 21),
        [1403] = new(2024, 3, 20),
        [1404] = new(2025, 3, 20),
        [1405] = new(2026, 3, 20),
        [1406] = new(2027, 3, 21),
        [1407] = new(2028, 3, 20),
        [1408] = new(2029, 3, 20),
        [1409] = new(2030, 3, 20),
        [1410] = new(2031, 3, 20),
        [1411] = new(2032, 3, 20),
        [1412] = new(2033, 3, 20),
        [1413] = new(2034, 3, 20),
        [1414] = new(2035, 3, 20),
        [1415] = new(2036, 3, 20),
    };

    private static readonly int[] SupportedYears =
        NowruzByPersianYear.Keys.OrderBy(year => year).ToArray();

    // Months 1-6 (Farvardin to Shahrivar) have 31 days each, so Mehr starts
    // 6 * 31 days after Nowruz.
    private const int DaysBeforeMehr = 186;

    public (DateOnly StartPeriod, DateOnly EndPeriod) GetMonthRange(int persianYear, int persianMonth)
    {
        if (persianMonth is < 1 or > 12)
            throw new ArgumentOutOfRangeException(
                nameof(persianMonth),
                "ماه شمسی باید بین 1 تا 12 باشد.");

        var start = GetNowruz(persianYear);

        if (persianMonth == 12 && !NowruzByPersianYear.ContainsKey(persianYear + 1))
            throw new ArgumentOutOfRangeException(
                nameof(persianYear),
                $"تقویم رسمی برای سال {persianYear} در دسترس نیست.");

        var daysInMonth = GetDaysInMonth(persianYear, persianMonth);
        var daysBeforeMonth = GetDaysBeforeMonth(persianMonth);

        return (
            start.AddDays(daysBeforeMonth),
            start.AddDays(daysBeforeMonth + daysInMonth - 1));
    }

    public int GetFridayCount(DateOnly periodStart, DateOnly periodEnd)
    {
        var fridayCount = 0;

        for (var date = periodStart; date <= periodEnd; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Friday)
                fridayCount++;
        }

        return fridayCount;
    }

    public int GetPersianMonth(DateOnly date)
    {
        var persianYear = GetPersianYear(date);
        var daysIntoYear = (date.ToTimeSpan() - GetNowruz(persianYear).ToTimeSpan()).Days;

        return daysIntoYear < DaysBeforeMehr
            ? 1 + daysIntoYear / 31
            : 7 + (daysIntoYear - DaysBeforeMehr) / 30;
    }

    public int GetPersianYear(DateOnly date)
    {
        var firstSupportedNowruz = NowruzByPersianYear[SupportedYears[0]];
        if (date < firstSupportedNowruz)
            throw new ArgumentOutOfRangeException(
                nameof(date),
                $"تقویم رسمی برای تاریخ {date:yyyy-MM-dd} در دسترس نیست.");

        for (var i = SupportedYears.Length - 1; i >= 0; i--)
        {
            var persianYear = SupportedYears[i];
            var nowruz = NowruzByPersianYear[persianYear];

            if (date < nowruz)
                continue;

            // For every year except the last one, the next year's Nowruz is
            // known, so the date is guaranteed to be inside this year.
            if (i < SupportedYears.Length - 1)
                return persianYear;

            // For the last supported year the following Nowruz is not known; a
            // Persian year lasts at most 366 days, so this is the safe bound.
            return date < nowruz.AddDays(366)
                ? persianYear
                : throw new ArgumentOutOfRangeException(
                    nameof(date),
                    $"تقویم رسمی برای تاریخ {date:yyyy-MM-dd} در دسترس نیست.");
        }

        throw new ArgumentOutOfRangeException(
            nameof(date),
            $"تقویم رسمی برای تاریخ {date:yyyy-MM-dd} در دسترس نیست.");
    }

    public int GetDaysInPersianYear(DateOnly date) =>
        IsLeapYear(GetPersianYear(date)) ? 366 : 365;

    private static int GetDaysBeforeMonth(int persianMonth) => persianMonth switch
    {
        1 => 0,
        2 => 31,
        3 => 62,
        4 => 93,
        5 => 124,
        6 => 155,
        7 => DaysBeforeMehr,
        8 => DaysBeforeMehr + 30,
        9 => DaysBeforeMehr + 60,
        10 => DaysBeforeMehr + 90,
        11 => DaysBeforeMehr + 120,
        _ => DaysBeforeMehr + 150
    };

    private static int GetDaysInMonth(int persianYear, int persianMonth) => persianMonth switch
    {
        < 7 => 31,
        < 12 => 30,
        _ => IsLeapYear(persianYear) ? 30 : 29
    };

    private static bool IsLeapYear(int persianYear) =>
        NowruzByPersianYear.TryGetValue(persianYear, out var nowruz)
        && NowruzByPersianYear.TryGetValue(persianYear + 1, out var nextNowruz)
        && (nextNowruz.ToTimeSpan() - nowruz.ToTimeSpan()).Days == 366;

    private static DateOnly GetNowruz(int persianYear) =>
        NowruzByPersianYear.TryGetValue(persianYear, out var nowruz)
            ? nowruz
            : throw new ArgumentOutOfRangeException(
                nameof(persianYear),
                $"تقویم رسمی برای سال {persianYear} در دسترس نیست.");
}
