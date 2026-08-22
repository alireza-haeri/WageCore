namespace Shared.Web.Tests.DateTimeHandling;

public class PersianDateTimeTests
{
    [Fact]
    public void ToUtc_ValidRawValue_ConvertsToCorrectUtcInstant()
    {
        // 1403/01/01 14:30 تهران (UTC+3:30) = 1403/01/01 11:00 UTC
        var sut = new PersianDateTime("1403/01/01 14:30");

        var result = sut.ToUtc();

        result.Should().Be(new DateTime(2024, 3, 20, 11, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void ToUtc_InvalidDatePart_ThrowsFormatException()
    {
        var sut = new PersianDateTime("invalid 14:30");

        var act = sut.ToUtc;

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void FromUtc_ValidDateTime_ProducesCorrectRawValue()
    {
        var utc = new DateTime(2024, 3, 20, 11, 0, 0, DateTimeKind.Utc);

        var result = PersianDateTime.FromUtc(utc);

        result.RawValue.Should().Be("1403/01/01 14:30");
    }

    [Fact]
    public void RoundTrip_FromUtcThenToUtc_PreservesExactInstant()
    {
        var original = new DateTime(2024, 3, 20, 11, 0, 0, DateTimeKind.Utc);

        var persianDateTime = PersianDateTime.FromUtc(original);
        var result = persianDateTime.ToUtc();

        result.Should().Be(original);
    }

    [Fact]
    public void ToDisplay_DefaultFormat_CombinesDateAndTime()
    {
        var sut = new PersianDateTime("1403/01/01 14:30");

        var result = sut.ToDisplay();

        result.Should().Be("1403/01/01 14:30");
    }

    [Fact]
    public void ToDisplay_TwelveHourFormat_ReturnsAmPmTime()
    {
        var sut = new PersianDateTime("1403/01/01 14:30");

        var result = sut.ToDisplay("yyyy/MM/dd", true);

        result.Should().Be("1403/01/01 02:30 PM");
    }

    [Fact]
    public void RawValue_ReflectsConstructorArgument()
    {
        var sut = new PersianDateTime("1403/05/25 09:00");

        sut.RawValue.Should().Be("1403/05/25 09:00");
    }
}