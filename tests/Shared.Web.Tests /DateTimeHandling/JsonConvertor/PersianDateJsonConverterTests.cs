namespace Shared.Web.Tests.DateTimeHandling.JsonConvertor;

public class PersianDateJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new PersianDateJsonConverter() }
    };

    [Fact]
    public void Deserialize_ValidDate_ReturnsPersianDateWithRawValue()
    {
        var json = "\"1403/05/25\"";

        var result = JsonSerializer.Deserialize<PersianDate>(json, _options);

        result.RawValue.Should().Be("1403/05/25");
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"invalid\"")]
    [InlineData("\"1403-05-25\"")]
    [InlineData("\"1403/13/01\"")]
    [InlineData("\"1403/01/32\"")]
    public void Deserialize_InvalidDate_ThrowsInvalidPersianDateException(string json)
    {
        var act = () => JsonSerializer.Deserialize<PersianDate>(json, _options);

        act.Should().Throw<InvalidPersianDateException>();
    }

    [Fact]
    public void Deserialize_NullString_ThrowsInvalidPersianDateException()
    {
        var json = "null";

        var act = () => JsonSerializer.Deserialize<PersianDate>(json, _options);

        act.Should().Throw<InvalidPersianDateException>();
    }

    [Fact]
    public void Serialize_PersianDate_WritesRawValueAsString()
    {
        var date = new PersianDate("1403/05/25");

        var json = JsonSerializer.Serialize(date, _options);

        json.Should().Be("\"1403/05/25\"");
    }

    [Fact]
    public void RoundTrip_SerializeThenDeserialize_PreservesRawValue()
    {
        var original = new PersianDate("1403/01/01");

        var json = JsonSerializer.Serialize(original, _options);
        var result = JsonSerializer.Deserialize<PersianDate>(json, _options);

        result.RawValue.Should().Be(original.RawValue);
    }
}