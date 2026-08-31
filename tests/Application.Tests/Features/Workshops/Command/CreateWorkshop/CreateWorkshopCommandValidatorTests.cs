namespace Application.Tests.Features.Workshops.Command.CreateWorkshop;

public class CreateWorkshopCommandValidatorTests
{
    private readonly CreateWorkshopCommandValidator _validator = new();

    private const string ValidName = "کارگاه نمونه";
    private const string ValidAddress = "تهران، خیابان نمونه، پلاک ۱۲۳";
    private static readonly DateOnly ValidRegistrationDate = DateOnly.FromDateTime(DateTime.Now);
    private const string ValidNationalId = "1234567890";
    private const string ValidPostalCode = "1234567890";
    private static readonly Guid ValidUserId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidCommandAndNullPostalCode_ShouldNotHaveAnyErrors()
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithValidCommandAndEmptyPostalCode_ShouldNotHaveAnyErrors()
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            "");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNameExactly2Characters_ShouldNotHaveErrors()
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            "اب",
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNameExactly200Characters_ShouldNotHaveErrors()
    {
        var name = new string('a', 200);
        var command = new CreateWorkshopCommand(ValidUserId,
            name,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAddressExactly10Characters_ShouldNotHaveErrors()
    {
        var address = new string('a', 10);
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            address,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAddressExactly1000Characters_ShouldNotHaveErrors()
    {
        var address = new string('a', 1000);
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            address,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithRegistrationDateToday_ShouldNotHaveErrors()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            today,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithRegistrationDatePast_ShouldNotHaveErrors()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            pastDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = new CreateWorkshopCommand(Guid.Empty,
            ValidName,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceName_ShouldHaveValidationError(string? name)
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            name!,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("ا")]
    public void Validate_WithNameLessThan2Characters_ShouldHaveValidationError(string name)
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            name,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameMoreThan200Characters_ShouldHaveValidationError()
    {
        var name = new string('a', 201);
        var command = new CreateWorkshopCommand(ValidUserId,
            name,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceAddress_ShouldHaveValidationError(string? address)
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            address!,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Theory]
    [InlineData("123456789")]
    public void Validate_WithAddressLessThan10Characters_ShouldHaveValidationError(string address)
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            address,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Validate_WithAddressMoreThan1000Characters_ShouldHaveValidationError()
    {
        var address = new string('a', 1001);
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            address,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Validate_WithRegistrationDateInFuture_ShouldHaveValidationError()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            futureDate,
            ValidNationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RegistrationDate);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceNationalId_ShouldHaveValidationError(string? nationalId)
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegistrationDate,
            nationalId!,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NationalId);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Validate_WithInvalidNationalId_ShouldHaveValidationError(string nationalId)
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegistrationDate,
            nationalId,
            "1234567890",
            ValidPostalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.NationalId);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Validate_WithInvalidPostalCode_ShouldHaveValidationError(string postalCode)
    {
        var command = new CreateWorkshopCommand(ValidUserId,
            ValidName,
            ValidAddress,
            ValidRegistrationDate,
            ValidNationalId,
            "1234567890",
            postalCode);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PostalCode);
    }
}
