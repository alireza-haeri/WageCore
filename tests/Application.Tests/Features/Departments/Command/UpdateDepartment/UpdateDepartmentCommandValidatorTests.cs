namespace Application.Tests.Features.Departments.Command.UpdateDepartment;

public class UpdateDepartmentCommandValidatorTests
{
    private readonly UpdateDepartmentCommandValidator _validator = new();

    private const string ValidName = "بخش نمونه";
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new UpdateDepartmentCommand(ValidUserId, ValidDepartmentId, ValidName);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNameExactly2Characters_ShouldNotHaveErrors()
    {
        var command = new UpdateDepartmentCommand(ValidUserId, ValidDepartmentId, "اب");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNameExactly100Characters_ShouldNotHaveErrors()
    {
        var name = new string('a', 100);
        var command = new UpdateDepartmentCommand(ValidUserId, ValidDepartmentId, name);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = new UpdateDepartmentCommand(Guid.Empty, ValidDepartmentId, ValidName);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyDepartmentId_ShouldHaveValidationError()
    {
        var command = new UpdateDepartmentCommand(ValidUserId, Guid.Empty, ValidName);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Validate_WithNullOrWhiteSpaceName_ShouldHaveValidationError(string? name)
    {
        var command = new UpdateDepartmentCommand(ValidUserId, ValidDepartmentId, name!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("ا")]
    public void Validate_WithNameLessThan2Characters_ShouldHaveValidationError(string name)
    {
        var command = new UpdateDepartmentCommand(ValidUserId, ValidDepartmentId, name);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameMoreThan100Characters_ShouldHaveValidationError()
    {
        var name = new string('a', 101);
        var command = new UpdateDepartmentCommand(ValidUserId, ValidDepartmentId, name);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
