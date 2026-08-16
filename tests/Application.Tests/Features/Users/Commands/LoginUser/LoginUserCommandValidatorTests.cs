namespace Application.Tests.Features.Users.Commands.LoginUser;

public class LoginUserCommandValidatorTests
{
    private readonly LoginUserCommandValidator _validator = new();

    private const string ValidPhoneNumber = "09123456789";
    private const string ValidEmail = "ali@gmail.com";
    private const string ValidPassword = "123456";

    [Fact]
    public void Validate_WithValidPhoneNumberAndPassword_ShouldNotHaveAnyErrors()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidEmailAndPassword_ShouldNotHaveAnyErrors()
    {
        var command = new LoginUserCommand(null, ValidEmail, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithBothPhoneAndEmailProvided_ShouldNotHaveAnyErrors()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, ValidEmail, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithBothPhoneAndEmailNullOrWhiteSpace_ShouldHaveValidationError(string? value)
    {
        var command = new LoginUserCommand(value!, value!, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrors();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpacePhoneNumberAndEmailProvided_ShouldNotHaveErrorForPhoneNumber(string? phoneNumber)
    {
        var command = new LoginUserCommand(phoneNumber!, ValidEmail, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [InlineData("0912345678")]
    [InlineData("091234567890")]
    [InlineData("12345678901")]
    [InlineData("0912345678a")]
    [InlineData("09123456789 ")]
    [InlineData(" 09123456789")]
    [InlineData("00989123456789")]
    [InlineData("+989123456789")]
    public void Validate_WithInvalidPhoneNumber_ShouldHaveValidationError(string phoneNumber)
    {
        var command = new LoginUserCommand(phoneNumber, null, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceEmailAndPhoneProvided_ShouldNotHaveErrorForEmail(string? email)
    {
        var command = new LoginUserCommand(ValidPhoneNumber, email!, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("user@domain.")]
    [InlineData("user@domain..com")]
    [InlineData("user name@domain.com")]
    [InlineData("user@domain.c")]
    [InlineData("user@.com")]
    [InlineData("user@domain.com.")]
    public void Validate_WithInvalidEmailFormat_ShouldHaveValidationError(string email)
    {
        var command = new LoginUserCommand(null, email, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithPasswordExactly6Characters_ShouldNotHaveErrors()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, "123456");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPasswordExactly50Characters_ShouldNotHaveErrors()
    {
        var password = new string('a', 50);
        var command = new LoginUserCommand(ValidPhoneNumber, null, password);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpacePassword_ShouldHaveValidationError(string? password)
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, password!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1")]
    [InlineData("")]
    public void Validate_WithPasswordShorterThan6Characters_ShouldHaveValidationError(string password)
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, password);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithPasswordLongerThan50Characters_ShouldHaveValidationError()
    {
        var password = new string('a', 51);
        var command = new LoginUserCommand(ValidPhoneNumber, null, password);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}