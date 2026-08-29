namespace Application.Tests.Features.PayrollRecords.Command.DeletePayrollRecord;

public class DeletePayrollRecordCommandValidatorTests
{
    private readonly DeletePayrollRecordCommandValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidPayrollRecordId = Guid.NewGuid();

    private static DeletePayrollRecordCommand CreateValidCommand(
        Guid? userId = null,
        Guid? employeeId = null,
        Guid? payrollRecordId = null) =>
        new(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            payrollRecordId ?? ValidPayrollRecordId);

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
    public void Validate_WithEmptyPayrollRecordId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(payrollRecordId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PayrollRecordId);
    }
}
