using Shared.Web.DateTimeHandling.CustomTypes;

namespace Shared.Web.DateTimeHandling.JsonConvertor;

public class PersianDateJsonConverter : JsonConverter<PersianDate>
{
    public override PersianDate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();

        if (string.IsNullOrWhiteSpace(raw) || !PersianCalendarHelper.TryParseDate(raw, out _))
            throw new InvalidPersianDateException(raw);

        return new PersianDate(raw!);
    }

    public override void Write(Utf8JsonWriter writer, PersianDate value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.RawValue);
}