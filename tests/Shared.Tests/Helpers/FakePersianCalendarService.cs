using Core.Abstractions.Services;

namespace Shared.Tests.Helpers;

/// <summary>
/// A calendar test double that distinguishes the hire date from "today".
/// The Persian year/month seen for the hire date and for all other dates
/// (i.e. today) can be configured independently.
/// </summary>
public class FakePersianCalendarService : IPersianCalendarService
{
    public DateOnly HireDate { get; set; }
    public int HireYear { get; set; } = 1405;
    public int HireMonth { get; set; } = 6;
    public int CurrentYear { get; set; } = 1405;
    public int CurrentMonth { get; set; } = 6;

    public (DateOnly StartPeriod, DateOnly EndPeriod) GetMonthRange(int persianYear, int persianMonth)
        => throw new NotSupportedException();

    public int GetFridayCount(DateOnly periodStart, DateOnly periodEnd)
        => throw new NotSupportedException();

    public int GetPersianMonth(DateOnly date) =>
        date == HireDate ? HireMonth : CurrentMonth;

    public int GetPersianYear(DateOnly date) =>
        date == HireDate ? HireYear : CurrentYear;

    public int GetDaysInPersianYear(DateOnly date) => 365;
}
