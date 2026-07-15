using Application.Features.Users.Commands;
using Shared.Tests.TestData;

namespace Application.Tests.Features.Users.Commands.RegisterOrLoginCommand;


public class RegisterOrLoginCommandValidatorTests
{
    private readonly RegisterOrLoginCommandValidator _validator = new();

    private const string ValidPhoneNumber = "09123456789";
    private const string ValidPassword = "123456";

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new RegisterOrLoginCommand(ValidPhoneNumber, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpacePhoneNumber_ShouldHaveValidationError(string? phoneNumber)
    {
        var command = new RegisterOrLoginCommand(phoneNumber!, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("0912345678")]
    [InlineData("091234567890")]
    [InlineData("12345")]
    public void Validate_WithInvalidPhoneNumberLength_ShouldHaveValidationError(string phoneNumber)
    {
        var command = new RegisterOrLoginCommand(phoneNumber, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpacePassword_ShouldHaveValidationError(string? password)
    {
        var command = new RegisterOrLoginCommand(ValidPhoneNumber, password!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1")]
    public void Validate_WithPasswordShorterThan6Characters_ShouldHaveValidationError(string password)
    {
        var command = new RegisterOrLoginCommand(ValidPhoneNumber, password);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordLongerThan50Characters_ShouldHaveValidationError()
    {
        var password = new string('a', 51);
        var command = new RegisterOrLoginCommand(ValidPhoneNumber, password);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}