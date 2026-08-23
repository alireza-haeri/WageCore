namespace Web.Ui.Services;

public static class StringHelperExtensions
{
    public static string ToPersianNumber(this string englishNumber)
    {
        return englishNumber
            .Replace("0", "۰")
            .Replace("1", "۱")
            .Replace("2", "۲")
            .Replace("3", "۳")
            .Replace("4", "۴")
            .Replace("5", "۵")
            .Replace("6", "۶")
            .Replace("7", "۷")
            .Replace("8", "۸")
            .Replace("9", "۹");
    }
    public static string ToPersianNumber(this int number)
    {
        return number.ToString().ToPersianNumber();
    }
}