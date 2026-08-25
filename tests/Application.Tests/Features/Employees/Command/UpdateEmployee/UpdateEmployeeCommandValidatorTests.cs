namespace Application.Tests.Features.Employees.Command.UpdateEmployee;

public class UpdateEmployeeCommandValidatorTests
{
    private readonly UpdateEmployeeCommandValidator _validator = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    private UpdateEmployeeCommand CreateValidCommand(EmployeeDto? employee = null, Guid? userId = null, Guid? employeeId = null)
    {
        return new UpdateEmployeeCommand(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            employee ?? _employeeBuilder.BuildEmployeeDto());
    }

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
        var command = CreateValidCommand(userId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(employeeId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Validate_WithNullEmployee_ShouldHaveValidationError()
    {
        var command = new UpdateEmployeeCommand(ValidUserId, ValidEmployeeId, null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee);
    }

    [Fact]
    public void Validate_WithInvalidPersonalCode_ShouldHaveValidationError()
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with { PersonalCode = "A-100" };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.PersonalCode);
    }

    [Fact]
    public void Validate_WithInvalidNationalCode_ShouldHaveValidationError()
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with { NationalCode = "123456789" };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.NationalCode);
    }
}
