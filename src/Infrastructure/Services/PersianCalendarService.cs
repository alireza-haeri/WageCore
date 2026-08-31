namespace Infrastructure.Services;

public class PersianCalendarService : IPersianCalendarService
{
    public (DateOnly StartPeriod, DateOnly EndPeriod) GetMonthRange(int persianYear, int persianMonth)
    {
        throw new NotImplementedException();
    }

    public int GetFridayCount(DateOnly periodStart, DateOnly periodEnd)
    {
        throw new NotImplementedException();
    }
}