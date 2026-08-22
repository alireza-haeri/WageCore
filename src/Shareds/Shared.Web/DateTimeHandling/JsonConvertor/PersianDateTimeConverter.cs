using Shared.Web.DateTimeHandling.CustomTypes;

namespace Shared.Web.DateTimeHandling.JsonConvertor;

public class PersianTimeJsonConverter : JsonConverter<PersianTime>
{
    private static readonly Regex TimePattern = new(@"^([01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);

    public override PersianTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();

        if (string.IsNullOrWhiteSpace(raw) || !TimePattern.IsMatch(raw))
            throw new InvalidPersianTimeException(raw);

        return new PersianTime(raw);
    }

    public override void Write(Utf8JsonWriter writer, PersianTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.RawValue);
}