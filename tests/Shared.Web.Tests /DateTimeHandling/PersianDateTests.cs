namespace Shared.Web.Tests.DateTimeHandling;

public class PersianDateTests
{
    [Fact]
    public void ToDateOnly_ValidRawValue_ConvertsToCorrectGregorianDate()
    {
        var sut = new PersianDate("1403/01/01");

        var result = sut.ToDateOnly();

        result.Should().Be(new DateOnly(2024, 3, 20));
    }

    [Fact]
    public void ToDateOnly_InvalidRawValue_ThrowsFormatException()
    {
        var sut = new PersianDate("invalid");

        var act = () => sut.ToDateOnly();

        act.Should().Throw<FormatException>();
    }

    [Fact]
    public void FromDateOnly_ValidDate_ProducesCorrectRawValue()
    {
        var date = new DateOnly(2024, 3, 20);

        var result = PersianDate.FromDateOnly(date);

        result.RawValue.Should().Be("1403/01/01");
    }

    [Fact]
    public void RoundTrip_FromDateOnlyThenToDateOnly_PreservesOriginalValue()
    {
        var original = new DateOnly(2024, 3, 20);

        var persianDate = PersianDate.FromDateOnly(original);
        var result = persianDate.ToDateOnly();

        result.Should().Be(original);
    }

    [Fact]
    public void ToDisplay_DefaultFormat_ReturnsSlashSeparatedValue()
    {
        var sut = new PersianDate("1403/01/01");

        var result = sut.ToDisplay();

        result.Should().Be("1403/01/01");
    }

    [Fact]
    public void ToDisplay_TextualFormat_ReturnsMonthName()
    {
        var sut = new PersianDate("1403/01/01");

        var result = sut.ToDisplay("d MMMM yyyy");

        result.Should().Be("1 فروردین 1403");
    }

    [Fact]
    public void RawValue_ReflectsConstructorArgument()
    {
        var sut = new PersianDate("1403/05/25");

        sut.RawValue.Should().Be("1403/05/25");
    }
}