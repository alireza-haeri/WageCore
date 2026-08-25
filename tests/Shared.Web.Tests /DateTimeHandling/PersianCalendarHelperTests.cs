namespace Shared.Web.Tests.DateTimeHandling;

public class PersianCalendarHelperTests
{
    [Fact]
    public void ToGregorianDate_NowruzFirstDay_ReturnsCorrectGregorianDate()
    {
        var result = PersianCalendarHelper.ToGregorianDate(1403, 1, 1);

        result.Should().Be(new DateOnly(2024, 3, 20));
    }

    [Fact]
    public void ToGregorianDate_LeapYearEsfand30th_DoesNotThrow()
    {
        // 1403 شمسی سال کبیسه است، اسفندش 30 روزه
        var act = () => PersianCalendarHelper.ToGregorianDate(1403, 12, 30);

        act.Should().NotThrow();
    }

    [Fact]
    public void ToGregorianDate_NonLeapYearEsfand30th_Throws()
    {
        // 1402 شمسی سال کبیسه نیست، اسفندش 29 روزه
        var act = () => PersianCalendarHelper.ToGregorianDate(1402, 12, 30);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToGregorianDate_Month7Day31_Throws()
    {
        // ماه‌های 7 تا 12 حداکثر 30 روز دارند
        var act = () => PersianCalendarHelper.ToGregorianDate(1403, 7, 31);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void FormatDate_DefaultFormat_ReturnsSlashSeparatedValue()
    {
        var date = new DateOnly(2024, 3, 20);

        var result = PersianCalendarHelper.FormatDate(date);

        result.Should().Be("1403/01/01");
    }

    [Fact]
    public void FormatDate_TextualFormat_ReturnsMonthNameVariant()
    {
        var date = new DateOnly(2024, 3, 20);

        var result = PersianCalendarHelper.FormatDate(date, "d MMMM yyyy");

        result.Should().Be("1 فروردین 1403");
    }

    [Fact]
    public void FormatDate_UnsupportedFormat_ThrowsNotSupportedException()
    {
        var date = new DateOnly(2024, 3, 20);

        var act = () => PersianCalendarHelper.FormatDate(date, "not-a-real-format");

        act.Should().Throw<NotSupportedException>();
    }

    [Theory]
    [InlineData("1403/01/01", true)]
    [InlineData("1403/12/30", true)]
    public void TryParseDate_ValidInputs_ReturnsTrue(string raw, bool expected)
    {
        var success = PersianCalendarHelper.TryParseDate(raw, out var date);

        success.Should().Be(expected);
        date.Should().NotBe(default);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("1403-01-01")]
    [InlineData("1403/13/01")]
    [InlineData("1403/01/32")]
    [InlineData("1402/12/30")]
    public void TryParseDate_InvalidInputs_ReturnsFalse(string? raw)
    {
        var success = PersianCalendarHelper.TryParseDate(raw!, out var date);

        success.Should().BeFalse();
        date.Should().Be(default);
    }

    [Fact]
    public void FormatTime_TwentyFourHour_ReturnsHHmm()
    {
        var time = new TimeOnly(14, 5);

        var result = PersianCalendarHelper.FormatTime(time);

        result.Should().Be("14:05");
    }

    [Fact]
    public void FormatTime_TwelveHour_ReturnsAmPmFormat()
    {
        var time = new TimeOnly(14, 5);

        var result = PersianCalendarHelper.FormatTime(time, twelveHour: true);

        result.Should().Be("02:05 PM");
    }

    [Fact]
    public void ToUtc_NowruzMidnightTehranTime_ConvertsToCorrectUtcInstant()
    {
        // 1403/01/01 00:00 به وقت تهران (UTC+3:30) = 2024-03-19 20:30 UTC
        var result = PersianCalendarHelper.ToUtc(1403, 1, 1);

        result.Should().Be(new DateTime(2024, 3, 19, 20, 30, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ToUtc_WithHourAndMinute_ConvertsToCorrectUtcInstant()
    {
        // 1403/01/01 14:30 تهران (UTC+3:30) = 1403/01/01 11:00 UTC
        var result = PersianCalendarHelper.ToUtc(1403, 1, 1, 14, 30);

        result.Should().Be(new DateTime(2024, 3, 20, 11, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void FormatDateTime_DefaultFormat_CombinesDateAndTimeInTehranLocalTime()
    {
        // 2024-03-20 11:00 UTC = 1403/01/01 14:30 تهران
        var utc = new DateTime(2024, 3, 20, 11, 0, 0, DateTimeKind.Utc);

        var result = PersianCalendarHelper.FormatDateTime(utc);

        result.Should().Be("1403/01/01 14:30");
    }

    [Fact]
    public void FormatDateTime_TwelveHourFormat_ReturnsAmPmTime()
    {
        var utc = new DateTime(2024, 3, 20, 11, 0, 0, DateTimeKind.Utc);

        var result = PersianCalendarHelper.FormatDateTime(utc, twelveHour: true);

        result.Should().Be("1403/01/01 02:30 PM");
    }
}