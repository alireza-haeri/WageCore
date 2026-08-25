namespace Shared.Web.Tests.DateTimeHandling.JsonConvertor;

public class PersianDateTimeJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new PersianDateTimeJsonConverter() }
    };

    [Fact]
    public void Deserialize_ValidDateTime_ReturnsPersianDateTimeWithRawValue()
    {
        var json = "\"1403/05/25 14:30\"";

        var result = JsonSerializer.Deserialize<PersianDateTime>(json, _options);

        result.RawValue.Should().Be("1403/05/25 14:30");
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"invalid\"")]
    [InlineData("\"1403/05/25\"")]
    [InlineData("\"14:30\"")]
    [InlineData("\"1403-05-25 14:30\"")]
    [InlineData("\"1403/13/01 14:30\"")]
    [InlineData("\"1403/05/25 24:00\"")]
    public void Deserialize_InvalidDateTime_ThrowsInvalidPersianDateTimeException(string json)
    {
        var act = () => JsonSerializer.Deserialize<PersianDateTime>(json, _options);

        act.Should().Throw<InvalidPersianDateTimeException>();
    }

    [Fact]
    public void Deserialize_InvalidCalendarDate_ThrowsInvalidPersianDateTimeException()
    {
        // فرمت رجکس درسته ولی روز 31 برای ماه 7 (مهر) که 30 روزه نامعتبره
        var json = "\"1403/07/31 10:00\"";

        var act = () => JsonSerializer.Deserialize<PersianDateTime>(json, _options);

        act.Should().Throw<InvalidPersianDateTimeException>();
    }

    [Fact]
    public void Deserialize_NullString_ThrowsInvalidPersianDateTimeException()
    {
        var json = "null";

        var act = () => JsonSerializer.Deserialize<PersianDateTime>(json, _options);

        act.Should().Throw<InvalidPersianDateTimeException>();
    }

    [Fact]
    public void Serialize_PersianDateTime_WritesRawValueAsString()
    {
        var dateTime = new PersianDateTime("1403/05/25 14:30");

        var json = JsonSerializer.Serialize(dateTime, _options);

        json.Should().Be("\"1403/05/25 14:30\"");
    }

    [Fact]
    public void RoundTrip_SerializeThenDeserialize_PreservesRawValue()
    {
        var original = new PersianDateTime("1403/01/01 09:00");

        var json = JsonSerializer.Serialize(original, _options);
        var result = JsonSerializer.Deserialize<PersianDateTime>(json, _options);

        result.RawValue.Should().Be(original.RawValue);
    }
}