namespace Shared.Kernel.Extensions;

public static partial class RegexExtensions
{
    [GeneratedRegex("^[0-9]+$")]
    public static partial Regex CheckPhoneNumberMustBeEnglishDigitsRegex();
}