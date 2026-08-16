namespace Shared.Kernel.Extensions;

public static partial class RegexExtensions
{
    [GeneratedRegex(@"^09\d{9}$")]
    public static partial Regex ValidIranianPhoneNumberRegex();
    
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$")]
    public static partial Regex ValidEmailRegex();
}