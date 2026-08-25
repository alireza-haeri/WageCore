namespace Shared.Web.Extensions;

public static class StringExtensions
{
    public static string? PersianTextNormalization(this string? input)
    {
        if (input is null) return null;
        
        var trimmed = input.Trim();
        var collapsed = Regex.Replace(trimmed, @"\s+", " ");
        var normalized = collapsed
            .Replace('ي', 'ی')
            .Replace('ك', 'ک');

        return normalized;
    }
}