namespace Application.Tests.Features.Employees.Command.DeleteEmployee;

public class DeleteEmployeeCommandValidatorTests
{
    private readonly DeleteEmployeeCommandValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new DeleteEmployeeCommand(ValidUserId, ValidEmployeeId);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = new DeleteEmployeeCommand(Guid.Empty, ValidEmployeeId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = new DeleteEmployeeCommand(ValidUserId, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Validate_WithBothIdsEmpty_ShouldHaveValidationErrors()
    {
        var command = new DeleteEmployeeCommand(Guid.Empty, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }
}
