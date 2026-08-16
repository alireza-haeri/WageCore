namespace Application.Tests.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator = new();

    private const string ValidPhoneNumber = "09123456789";
    private const string ValidEmail = "ali@gmail.com";
    private const string ValidPassword = "123456";
    private const string ValidFullName = "علی رضایی";

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOnlyPhoneNumberAndNoEmail_ShouldNotHaveAnyErrors()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, null, ValidFullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithOnlyEmailAndNoPhoneNumber_ShouldNotHaveAnyErrors()
    {
        var command = new RegisterUserCommand(null, ValidEmail, ValidFullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithFullNameExactly3Characters_ShouldNotHaveErrors()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, "abc", ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithFullNameExactly100Characters_ShouldNotHaveErrors()
    {
        var fullName = new string('a', 100);
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, fullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPasswordExactly6Characters_ShouldNotHaveErrors()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, "123456");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPasswordExactly50Characters_ShouldNotHaveErrors()
    {
        var password = new string('a', 50);
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, password);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithBothPhoneAndEmailNullOrWhiteSpace_ShouldHaveValidationError(string? value)
    {
        var command = new RegisterUserCommand(value!, value!, ValidFullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrors();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpacePhoneNumberAndEmailProvided_ShouldNotHaveErrorForPhoneNumber(string? phoneNumber)
    {
        var command = new RegisterUserCommand(phoneNumber!, ValidEmail, ValidFullName, ValidPassword);

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
        var command = new RegisterUserCommand(phoneNumber, null, ValidFullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("شماره تلفن باید با ۰۹ شروع شده و دقیقاً ۱۱ رقم انگلیسی باشد.");
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceEmailAndPhoneProvided_ShouldNotHaveErrorForEmail(string? email)
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, email!, ValidFullName, ValidPassword);

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
        var command = new RegisterUserCommand(null, email, ValidFullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceFullName_ShouldHaveValidationError(string? fullName)
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, fullName!, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("نام و نام خانوادگی اجباری است.");
    }

    [Theory]
    [InlineData("اب")]
    [InlineData("لف")]
    [InlineData("")]
    public void Validate_WithFullNameLessThan3Characters_ShouldHaveValidationError(string fullName)
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, fullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName)
            .WithErrorMessage("نام و نام خانوادگی نمیتواند کمتر از 3 کاراکتر باشد.");
    }

    [Fact]
    public void Validate_WithFullNameMoreThan100Characters_ShouldHaveValidationError()
    {
        var fullName = new string('a', 101);
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, fullName, ValidPassword);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpacePassword_ShouldHaveValidationError(string? password)
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, password!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("رمز عبور اجباری است.");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1")]
    [InlineData("")]
    public void Validate_WithPasswordShorterThan6Characters_ShouldHaveValidationError(string password)
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, password);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("رمز عبور نمیتواند کمتر از 6 کاراکتر باشد.");
    }

    [Fact]
    public void Validate_WithPasswordLongerThan50Characters_ShouldHaveValidationError()
    {
        var password = new string('a', 51);
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, password);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("رمز عبور نمیتواند بیشتر از 50 کاراکتر باشد.");
    }
}