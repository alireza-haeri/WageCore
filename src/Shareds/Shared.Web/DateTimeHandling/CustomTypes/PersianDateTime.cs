namespace Shared.Web.DateTimeHandling.CustomTypes;

public readonly struct PersianDateTime
{
    public string RawValue { get; }   // "yyyy/MM/dd HH:mm"

    public PersianDateTime(string rawValue) => RawValue = rawValue;

    public DateTime ToUtc()
    {
        var parts = RawValue.Split(' ');
        if (!PersianCalendarHelper.TryParseDate(parts[0], out _))
            throw new FormatException("فرمت تاریخ نامعتبر است");

        var datePartRaw = parts[0].Split('/');
        var timePart = parts[1].Split(':');

        return PersianCalendarHelper.ToUtc(
            int.Parse(datePartRaw[0]),
            int.Parse(datePartRaw[1]),
            int.Parse(datePartRaw[2]),
            int.Parse(timePart[0]),
            int.Parse(timePart[1]));
    }

    public static PersianDateTime FromUtc(DateTime utc) => new(PersianCalendarHelper.FormatDateTime(utc));

    public string ToDisplay(string dateFormat, bool twelveHour) 
        => PersianCalendarHelper.FormatDateTime(ToUtc(), dateFormat, twelveHour);

    [Obsolete("Specify the date format and time style explicitly. Relying on the default values is discouraged.", error: false)]
    public string ToDisplay() => ToDisplay("yyyy/MM/dd", false);
}