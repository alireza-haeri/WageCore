using Shared.Web.DateTimeHandling.CustomTypes;

namespace Shared.Web.DateTimeHandling.JsonConvertor;

public class PersianDateTimeJsonConverter : JsonConverter<PersianDateTime>
{
    private static readonly Regex DateTimePattern =
        new(@"^1[34]\d{2}/(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01]) ([01]\d|2[0-3]):[0-5]\d$", RegexOptions.Compiled);

    public override PersianDateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var raw = reader.GetString();

        if (string.IsNullOrWhiteSpace(raw) || !DateTimePattern.IsMatch(raw))
            throw new InvalidPersianDateTimeException(raw);

        var datePart = raw.Split(' ')[0];
        if (!PersianCalendarHelper.TryParseDate(datePart, out _))
            throw new InvalidPersianDateTimeException(raw);

        return new PersianDateTime(raw);
    }

    public override void Write(Utf8JsonWriter writer, PersianDateTime value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.RawValue);
}