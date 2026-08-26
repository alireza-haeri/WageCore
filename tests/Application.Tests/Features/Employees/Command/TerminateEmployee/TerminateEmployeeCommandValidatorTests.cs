namespace Application.Tests.Features.Employees.Command.TerminateEmployee;

public class TerminateEmployeeCommandValidatorTests
{
    private readonly TerminateEmployeeCommandValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly DateOnly ValidTerminationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

    private static TerminateEmployeeCommand CreateValidCommand() =>
        new(ValidUserId, ValidEmployeeId, ValidTerminationDate);

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with { UserId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with { EmployeeId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Validate_WithNullTerminationDate_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with { TerminationDate = null };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TerminationDate)
            .WithErrorMessage("تاریخ ترک کار اجباری است.");
    }

    [Fact]
    public void Validate_WithFutureTerminationDate_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with { TerminationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.TerminationDate)
            .WithErrorMessage("تاریخ ترک کار نباید برای آینده باشد.");
    }

    [Fact]
    public void Validate_WithTodayTerminationDate_ShouldNotHaveValidationError()
    {
        var command = CreateValidCommand() with { TerminationDate = DateOnly.FromDateTime(DateTime.Now) };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.TerminationDate);
    }
}
