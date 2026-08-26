namespace Application.Tests.Features.Employees.Command.RehireEmployee;

public class RehireEmployeeCommandValidatorTests
{
    private readonly RehireEmployeeCommandValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private static readonly DateOnly ValidHireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

    private static RehireEmployeeCommand CreateValidCommand() =>
        new(ValidUserId, ValidEmployeeId, ValidDepartmentId, ValidHireDate);

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
    public void Validate_WithEmptyDepartmentId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with { DepartmentId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
    }

    [Fact]
    public void Validate_WithNullHireDate_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with { HireDate = null };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.HireDate)
            .WithErrorMessage("تاریخ استخدام اجباری است.");
    }

    [Fact]
    public void Validate_WithFutureHireDate_ShouldHaveValidationError()
    {
        var command = CreateValidCommand() with { HireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1)) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.HireDate)
            .WithErrorMessage("تاریخ استخدام نباید برای آینده باشد.");
    }

    [Fact]
    public void Validate_WithTodayHireDate_ShouldNotHaveValidationError()
    {
        var command = CreateValidCommand() with { HireDate = DateOnly.FromDateTime(DateTime.Now) };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.HireDate);
    }
}
