using System.Globalization;

namespace Infrastructure.Services;

public class PersianCalendarService : IPersianCalendarService
{
    private static readonly PersianCalendar PersianCalendar = new();

    public (DateOnly StartPeriod, DateOnly EndPeriod) GetMonthRange(int persianYear, int persianMonth)
    {
        throw new NotImplementedException();
    }

    public int GetFridayCount(DateOnly periodStart, DateOnly periodEnd)
    {
        throw new NotImplementedException();
    }

    public int GetPersianMonth(DateOnly date) =>
        PersianCalendar.GetMonth(date.ToDateTime(TimeOnly.MinValue));
}
