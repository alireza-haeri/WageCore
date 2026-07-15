namespace Core.Tests.Domain.Users;

public class CreateUserTests
{
    private readonly UserBuilder _builder = new();

    [Fact]
    public void Create_WithValidPhoneNumber_ShouldReturnSuccess()
    {
        var phoneNumber = "09123456789";

        var result = _builder.WithPhoneNumber(phoneNumber).CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.PhoneNumber.Should().Be(phoneNumber);
        }
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Create_WithNullOrWithSpacePhoneNumber_ShouldFail(string? phoneNumber)
    {
        var result = _builder.WithPhoneNumber(phoneNumber).CreateResult();
        result.ShouldBeFailure();
    }

    [Theory]
    [MemberData(nameof(PhoneNumberTestData.InvalidLengthPhoneNumbers), MemberType = typeof(PhoneNumberTestData))]
    public void Create_WithInvalidLengthPhoneNumber_ShouldFail(string phoneNumber)
    {
        var result = _builder.WithPhoneNumber(phoneNumber).CreateResult();
        result.ShouldBeFailure("11 رقم");
    }

    [Theory]
    [MemberData(nameof(PhoneNumberTestData.NoneEnglishDigitsOrInvalidCharacters) , MemberType = typeof(PhoneNumberTestData))]
    public void Create_WithNonEnglishDigitsOrInvalidCharactersPhoneNumber_ShouldFail(string phoneNumber)
    {
        var result = _builder.WithPhoneNumber(phoneNumber).CreateResult();
        result.ShouldBeFailure("اعداد");
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();
        result.ShouldBeFailure("شناسه");
    }
}