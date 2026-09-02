using System.Globalization;

namespace Infrastructure.Services;

public class PersianCalendarService : IPersianCalendarService
{
    private static readonly PersianCalendar PersianCalendar = new();

    public (DateOnly StartPeriod, DateOnly EndPeriod) GetMonthRange(int persianYear, int persianMonth)
    {
        var daysInMonth = PersianCalendar.GetDaysInMonth(persianYear, persianMonth);

        var startDate = PersianCalendar.ToDateTime(persianYear, persianMonth, 1, 0, 0, 0, 0);
        var endDate = PersianCalendar.ToDateTime(persianYear, persianMonth, daysInMonth, 0, 0, 0, 0);

        return (DateOnly.FromDateTime(startDate), DateOnly.FromDateTime(endDate));
    }

    public int GetFridayCount(DateOnly periodStart, DateOnly periodEnd)
    {
        var fridayCount = 0;

        for (var date = periodStart; date <= periodEnd; date = date.AddDays(1))
        {
            if (PersianCalendar.GetDayOfWeek(date.ToDateTime(TimeOnly.MinValue)) == DayOfWeek.Friday)
                fridayCount++;
        }

        return fridayCount;
    }

    public int GetPersianMonth(DateOnly date) =>
        PersianCalendar.GetMonth(date.ToDateTime(TimeOnly.MinValue));
}
