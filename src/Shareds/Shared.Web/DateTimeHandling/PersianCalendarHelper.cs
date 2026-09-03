namespace Shared.Web.DateTimeHandling;

public static class PersianCalendarHelper
{
    private static readonly PersianCalendar Pc = new();
    private static readonly TimeZoneInfo IranTimeZone = ResolveIranTimeZone();

    private static TimeZoneInfo ResolveIranTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Tehran");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");
        }
    }
    
    public static DateOnly ToGregorianDate(int year, int month, int day)
        => DateOnly.FromDateTime(Pc.ToDateTime(year, month, day, 0, 0, 0, 0));

    public static string FormatDate(DateOnly date, string format = "yyyy/MM/dd")
    {
        var dt = date.ToDateTime(TimeOnly.MinValue);
        return format switch
        {
            "yyyy/MM/dd" => $"{Pc.GetYear(dt):0000}/{Pc.GetMonth(dt):00}/{Pc.GetDayOfMonth(dt):00}",
            "d MMMM yyyy" => $"{Pc.GetDayOfMonth(dt)} {PersianMonthName(Pc.GetMonth(dt))} {Pc.GetYear(dt)}",
            "MMMM yyyy" => $"{PersianMonthName(Pc.GetMonth(dt))} {Pc.GetYear(dt)}",
            _ => throw new NotSupportedException($"فرمت {format} پشتیبانی نمی‌شود")
        };
    }

    public static bool TryParseDate(string? raw, out DateOnly date)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        if (!Regex.IsMatch(raw, @"^1[34]\d{2}/(0[1-9]|1[0-2])/(0[1-9]|[12]\d|3[01])$")) return false;

        var parts = raw.Split('/');
        try
        {
            date = ToGregorianDate(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    public static string FormatTime(TimeOnly time, bool twelveHour = false)
        => twelveHour ? time.ToString("hh:mm tt") : time.ToString("HH:mm");

    public static DateTime ToUtc(int year, int month, int day, int hour = 0, int minute = 0)
    {
        var unspecified = DateTime.SpecifyKind(
            Pc.ToDateTime(year, month, day, hour, minute, 0, 0),
            DateTimeKind.Unspecified);

        return TimeZoneInfo.ConvertTimeToUtc(unspecified, IranTimeZone);
    }

    public static string FormatDateTime(DateTime utc, string dateFormat = "yyyy/MM/dd", bool twelveHour = false)
    {
        var tehranLocal = TimeZoneInfo.ConvertTimeFromUtc(utc, IranTimeZone);
        var date = DateOnly.FromDateTime(tehranLocal);
        var time = TimeOnly.FromDateTime(tehranLocal);
        return $"{FormatDate(date, dateFormat)} {FormatTime(time, twelveHour)}";
    }

    private static string PersianMonthName(int month) => month switch
    {
        1 => "فروردین", 2 => "اردیبهشت", 3 => "خرداد", 4 => "تیر",
        5 => "مرداد", 6 => "شهریور", 7 => "مهر", 8 => "آبان",
        9 => "آذر", 10 => "دی", 11 => "بهمن", 12 => "اسفند",
        _ => throw new ArgumentOutOfRangeException(nameof(month))
    };
}