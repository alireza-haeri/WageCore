namespace Shared.Web.DateTimeHandling.CustomTypes;

public readonly struct PersianDate(string rawValue)
{
    public string RawValue { get; } = rawValue;

    public DateOnly ToDateOnly()
    {
        if (!PersianCalendarHelper.TryParseDate(RawValue, out var date))
            throw new FormatException("فرمت تاریخ نامعتبر است");
        return date;
    }

    public static PersianDate FromDateOnly(DateOnly date) => new(PersianCalendarHelper.FormatDate(date));

    public string ToDisplay(string format) => PersianCalendarHelper.FormatDate(ToDateOnly(), format);

    [Obsolete("Specify the date format explicitly. Relying on the default format is discouraged.", error: false)]
    public string ToDisplay() => ToDisplay("yyyy/MM/dd");
}