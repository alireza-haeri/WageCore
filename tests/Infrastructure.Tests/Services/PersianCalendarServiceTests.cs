namespace Infrastructure.Tests.Services;

public class PersianCalendarServiceTests
{
    private readonly PersianCalendarService _service = new();

    #region GetMonthRange

    [Fact]
    public void GetMonthRange_ForFarvardinOf1404_ShouldReturnItsGregorianBounds()
    {
        var result = _service.GetMonthRange(1404, 1);

        result.StartPeriod.Should().Be(new DateOnly(2025, 3, 20));
        result.EndPeriod.Should().Be(new DateOnly(2025, 4, 19));
    }

    [Fact]
    public void GetMonthRange_ForEsfandOfNonLeapYear1404_ShouldReturnA29DayMonth()
    {
        var result = _service.GetMonthRange(1404, 12);

        result.StartPeriod.Should().Be(new DateOnly(2026, 2, 19));
        result.EndPeriod.Should().Be(new DateOnly(2026, 3, 19));
    }

    [Fact]
    public void GetMonthRange_ForFarvardinOf1405_ShouldStartOnTheNextNowruz()
    {
        var result = _service.GetMonthRange(1405, 1);

        result.StartPeriod.Should().Be(new DateOnly(2026, 3, 20));
        result.EndPeriod.Should().Be(new DateOnly(2026, 4, 19));
    }

    #endregion

    #region GetFridayCount

    [Fact]
    public void GetFridayCount_ForFarvardin1404_ShouldCountFiveFridays()
    {
        var result = _service.GetFridayCount(
            new DateOnly(2025, 3, 20),
            new DateOnly(2025, 4, 19));

        result.Should().Be(5);
    }

    [Fact]
    public void GetFridayCount_ForEsfand1404_ShouldCountFourFridays()
    {
        var result = _service.GetFridayCount(
            new DateOnly(2026, 2, 19),
            new DateOnly(2026, 3, 19));

        result.Should().Be(4);
    }

    [Fact]
    public void GetFridayCount_WhenTheRangeContainsAFriday_ShouldCountIt()
    {
        var result = _service.GetFridayCount(
            new DateOnly(2025, 3, 21),
            new DateOnly(2025, 3, 21));

        result.Should().Be(1);
    }

    [Fact]
    public void GetFridayCount_WhenTheRangeHasNoFriday_ShouldReturnZero()
    {
        var result = _service.GetFridayCount(
            new DateOnly(2025, 3, 22),
            new DateOnly(2025, 3, 24));

        result.Should().Be(0);
    }

    #endregion

    #region GetPersianMonth

    [Fact]
    public void GetPersianMonth_OnNowruz_ShouldReturnOne()
    {
        var result = _service.GetPersianMonth(new DateOnly(2025, 3, 21));

        result.Should().Be(1);
    }

    [Fact]
    public void GetPersianMonth_InTheMiddleOfTheYear_ShouldReturnTheMonthNumber()
    {
        var result = _service.GetPersianMonth(new DateOnly(2025, 11, 21));

        result.Should().Be(9);
    }

    [Fact]
    public void GetPersianMonth_OnTheLastDayOfTheYear_ShouldReturnTwelve()
    {
        var result = _service.GetPersianMonth(new DateOnly(2026, 3, 19));

        result.Should().Be(12);
    }

    #endregion

    #region GetPersianYear

    [Fact]
    public void GetPersianYear_OnNowruz_ShouldReturnTheNewYear()
    {
        var result = _service.GetPersianYear(new DateOnly(2026, 3, 20));

        result.Should().Be(1405);
    }

    [Fact]
    public void GetPersianYear_InTheMiddleOfTheYear_ShouldReturnTheYearNumber()
    {
        var result = _service.GetPersianYear(new DateOnly(2025, 11, 21));

        result.Should().Be(1404);
    }

    [Fact]
    public void GetPersianYear_OnTheLastDayOfTheYear_ShouldReturnThatYear()
    {
        var result = _service.GetPersianYear(new DateOnly(2026, 3, 19));

        result.Should().Be(1404);
    }

    #endregion

    #region GetDaysInPersianYear

    [Fact]
    public void GetDaysInPersianYear_ForADateIn1404_ShouldReturn365()
    {
        var result = _service.GetDaysInPersianYear(new DateOnly(2025, 10, 1));

        result.Should().Be(365);
    }

    [Fact]
    public void GetDaysInPersianYear_ForTheLastDayOf1404_ShouldReturn365()
    {
        var result = _service.GetDaysInPersianYear(new DateOnly(2026, 3, 19));

        result.Should().Be(365);
    }

    [Fact]
    public void GetDaysInPersianYear_ForADateInALeapYear_ShouldReturn366()
    {
        var persianCalendar = new System.Globalization.PersianCalendar();
        var leapYear = Enumerable.Range(1400, 100).First(year => persianCalendar.IsLeapYear(year));
        var dateInLeapYear = DateOnly.FromDateTime(
            persianCalendar.ToDateTime(leapYear, 6, 1, 0, 0, 0, 0));

        var result = _service.GetDaysInPersianYear(dateInLeapYear);

        result.Should().Be(366);
    }

    [Fact]
    public void GetDaysInPersianYear_ForADateInACommonYear_ShouldReturn365()
    {
        var persianCalendar = new System.Globalization.PersianCalendar();
        var commonYear = Enumerable.Range(1400, 100).First(year => !persianCalendar.IsLeapYear(year));
        var dateInCommonYear = DateOnly.FromDateTime(
            persianCalendar.ToDateTime(commonYear, 6, 1, 0, 0, 0, 0));

        var result = _service.GetDaysInPersianYear(dateInCommonYear);

        result.Should().Be(365);
    }

    [Fact]
    public void GetDaysInPersianYear_ShouldAgreeWithTheLengthOfTheYearSpanningTheDate()
    {
        var sampleDate = new DateOnly(2025, 8, 15);

        var yearOfSample = new System.Globalization.PersianCalendar()
            .GetYear(sampleDate.ToDateTime(TimeOnly.MinValue));
        var yearStart = _service.GetMonthRange(yearOfSample, 1).StartPeriod;
        var yearEnd = _service.GetMonthRange(yearOfSample, 12).EndPeriod;
        var yearLength = yearEnd.DayNumber - yearStart.DayNumber + 1;

        _service.GetDaysInPersianYear(sampleDate).Should().Be(yearLength);
    }

    #endregion
}
