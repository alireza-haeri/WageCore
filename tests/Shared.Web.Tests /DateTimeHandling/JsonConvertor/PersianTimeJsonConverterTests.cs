namespace Shared.Web.Tests.DateTimeHandling.JsonConvertor;

public class PersianTimeJsonConverterTests
{
    private readonly JsonSerializerOptions _options = new()
    {
        Converters = { new PersianTimeJsonConverter() }
    };

    [Fact]
    public void Deserialize_ValidTime_ReturnsPersianTimeWithRawValue()
    {
        var json = "\"14:30\"";

        var result = JsonSerializer.Deserialize<PersianTime>(json, _options);

        result.RawValue.Should().Be("14:30");
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"invalid\"")]
    [InlineData("\"24:00\"")]
    [InlineData("\"14:60\"")]
    [InlineData("\"1430\"")]
    [InlineData("\"14:3\"")]
    public void Deserialize_InvalidTime_ThrowsInvalidPersianTimeException(string json)
    {
        var act = () => JsonSerializer.Deserialize<PersianTime>(json, _options);

        act.Should().Throw<InvalidPersianTimeException>();
    }

    [Fact]
    public void Deserialize_NullString_ThrowsInvalidPersianTimeException()
    {
        var json = "null";

        var act = () => JsonSerializer.Deserialize<PersianTime>(json, _options);

        act.Should().Throw<InvalidPersianTimeException>();
    }

    [Fact]
    public void Serialize_PersianTime_WritesRawValueAsString()
    {
        var time = new PersianTime("14:30");

        var json = JsonSerializer.Serialize(time, _options);

        json.Should().Be("\"14:30\"");
    }

    [Fact]
    public void RoundTrip_SerializeThenDeserialize_PreservesRawValue()
    {
        var original = new PersianTime("08:05");

        var json = JsonSerializer.Serialize(original, _options);
        var result = JsonSerializer.Deserialize<PersianTime>(json, _options);

        result.RawValue.Should().Be(original.RawValue);
    }
}