namespace Shared.Web.Tests.DateTimeHandling;

public class PersianTimeTests
{
    [Fact]
    public void ToTimeOnly_ValidRawValue_ConvertsCorrectly()
    {
        var sut = new PersianTime("14:30");

        var result = sut.ToTimeOnly();

        result.Should().Be(new TimeOnly(14, 30));
    }

    [Fact]
    public void FromTimeOnly_ValidTime_ProducesCorrectRawValue()
    {
        var time = new TimeOnly(9, 5);

        var result = PersianTime.FromTimeOnly(time);

        result.RawValue.Should().Be("09:05");
    }

    [Fact]
    public void RoundTrip_FromTimeOnlyThenToTimeOnly_PreservesOriginalValue()
    {
        var original = new TimeOnly(23, 59);

        var persianTime = PersianTime.FromTimeOnly(original);
        var result = persianTime.ToTimeOnly();

        result.Should().Be(original);
    }

    [Fact]
    public void ToDisplay_TwentyFourHourFormat_ReturnsHHmm()
    {
        var sut = new PersianTime("14:30");

        var result = sut.ToDisplay();

        result.Should().Be("14:30");
    }

    [Fact]
    public void ToDisplay_TwelveHourFormat_ReturnsAmPmFormat()
    {
        var sut = new PersianTime("14:30");

        var result = sut.ToDisplay(twelveHour: true);

        result.Should().Be("02:30 PM");
    }

    [Fact]
    public void RawValue_ReflectsConstructorArgument()
    {
        var sut = new PersianTime("08:15");

        sut.RawValue.Should().Be("08:15");
    }
}