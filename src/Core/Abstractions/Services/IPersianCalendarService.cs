namespace Core.Abstractions.Services;

public interface IPersianCalendarService
{
    (DateOnly StartPeriod, DateOnly EndPeriod) GetMonthRange(int persianYear, int persianMonth);

    int GetFridayCount(DateOnly periodStart, DateOnly periodEnd);

    int GetPersianMonth(DateOnly date);
}
