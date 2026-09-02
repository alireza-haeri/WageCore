namespace Core.Abstractions.Services;

public interface IPersianCalendarService
{
    (DateOnly StartPeriod, DateOnly EndPeriod) GetMonthRange(int persianYear, int persianMonth);

    int GetFridayCount(DateOnly periodStart, DateOnly periodEnd);

    int GetPersianMonth(DateOnly date);

    // Returns 365 or 366 depending on whether the Persian year that contains
    // the given date is a leap year.
    int GetDaysInPersianYear(DateOnly date);
}
