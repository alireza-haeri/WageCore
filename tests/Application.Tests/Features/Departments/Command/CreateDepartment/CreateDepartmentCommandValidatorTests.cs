namespace Application.Tests.Features.Departments.Command.CreateDepartment;

public class CreateDepartmentCommandValidatorTests
{
    private readonly CreateDepartmentCommandValidator _validator = new();

    private const string ValidName = "دپارتمان نمونه";
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, ValidName);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNameExactly2Characters_ShouldNotHaveErrors()
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, "اب");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNameExactly100Characters_ShouldNotHaveErrors()
    {
        var name = new string('a', 100);
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, name);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = new CreateDepartmentCommand(Guid.Empty, ValidWorkshopId, ValidName);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyWorkshopId_ShouldHaveValidationError()
    {
        var command = new CreateDepartmentCommand(ValidUserId, Guid.Empty, ValidName);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.WorkshopId);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceName_ShouldHaveValidationError(string? name)
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, name!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("ا")]
    public void Validate_WithNameLessThan2Characters_ShouldHaveValidationError(string name)
    {
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, name);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameMoreThan100Characters_ShouldHaveValidationError()
    {
        var name = new string('a', 101);
        var command = new CreateDepartmentCommand(ValidUserId, ValidWorkshopId, name);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
