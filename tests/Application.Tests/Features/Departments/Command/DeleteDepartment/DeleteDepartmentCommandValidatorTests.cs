namespace Application.Tests.Features.Departments.Command.DeleteDepartment;

public class DeleteDepartmentCommandValidatorTests
{
    private readonly DeleteDepartmentCommandValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new DeleteDepartmentCommand(ValidUserId, ValidDepartmentId);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = new DeleteDepartmentCommand(Guid.Empty, ValidDepartmentId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyDepartmentId_ShouldHaveValidationError()
    {
        var command = new DeleteDepartmentCommand(ValidUserId, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void Validate_WithBothIdsEmpty_ShouldHaveValidationErrors()
    {
        var command = new DeleteDepartmentCommand(Guid.Empty, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }
}
