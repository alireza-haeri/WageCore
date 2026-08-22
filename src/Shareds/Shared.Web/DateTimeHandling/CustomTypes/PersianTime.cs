namespace Shared.Web.DateTimeHandling.CustomTypes;

public readonly struct PersianTime(string rawValue)
{
    public string RawValue { get; } = rawValue;

    public TimeOnly ToTimeOnly()
    {
        var parts = RawValue.Split(':');
        return new TimeOnly(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    public static PersianTime FromTimeOnly(TimeOnly time) => new(time.ToString("HH:mm"));

    public string ToDisplay(bool twelveHour) => PersianCalendarHelper.FormatTime(ToTimeOnly(), twelveHour);

    [Obsolete("Specify twelveHour explicitly. Relying on the default value is discouraged.", error: false)]
    public string ToDisplay() => ToDisplay(false);
}