namespace Shared.Tests.TestData;

public static class PhoneNumberTestData
{
    public static TheoryData<string> InvalidLengthPhoneNumbers =>
        new() { "0912345678", "091234567890", "12345" };

    public static TheoryData<string> NoneEnglishDigitsOrInvalidCharacters =>
        new() { "0912345678a", "۰9123456789", "0912-345-67" };
}