namespace Core.Abstractions.Services;

public interface IPersianCalendarService
{
    (DateOnly StartPeriod, DateOnly EndPeriod) GetMonthRange(int persianYear, int persianMonth);
}
