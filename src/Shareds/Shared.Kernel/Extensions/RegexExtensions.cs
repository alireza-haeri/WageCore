namespace Shared.Kernel.Extensions;

public static partial class RegexExtensions
{
    [GeneratedRegex(@"^09[0-9]{9}$")]
    public static partial Regex ValidIranianPhoneNumberRegex();
    
    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$")]
    public static partial Regex ValidEmailRegex();

    [GeneratedRegex(@"^[0-9]{10}$")]
    public static partial Regex ValidNationalIdRegex();

    [GeneratedRegex(@"^[0-9]{10}$")]
    public static partial Regex ValidPostalCodeRegex();

    [GeneratedRegex(@"^[0-9]{1,20}$")]
    public static partial Regex ValidSocialSecurityNumberRegex();

    [GeneratedRegex(@"^[0-9]{1,20}$")]
    public static partial Regex ValidEconomicCodeRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9]{1,20}$")]
    public static partial Regex ValidEmployeePersonalCodeRegex();

    [GeneratedRegex(@"^IR[0-9]{24}$", RegexOptions.IgnoreCase)]
    public static partial Regex ValidIranianIbanRegex();
}