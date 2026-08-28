namespace Application.Tests.Features.EmployeeSalaryProfiles.Command.DeleteEmployeeSalaryProfile;

public class DeleteEmployeeSalaryProfileCommandValidatorTests
{
    private readonly DeleteEmployeeSalaryProfileCommandValidator _validator = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveAnyErrors()
    {
        var command = new DeleteEmployeeSalaryProfileCommand(ValidUserId, ValidEmployeeId, ValidSalaryProfileId);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        var command = new DeleteEmployeeSalaryProfileCommand(Guid.Empty, ValidEmployeeId, ValidSalaryProfileId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyEmployeeId_ShouldHaveValidationError()
    {
        var command = new DeleteEmployeeSalaryProfileCommand(ValidUserId, Guid.Empty, ValidSalaryProfileId);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
    }

    [Fact]
    public void Validate_WithEmptySalaryProfileId_ShouldHaveValidationError()
    {
        var command = new DeleteEmployeeSalaryProfileCommand(ValidUserId, ValidEmployeeId, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeSalaryProfileId);
    }

    [Fact]
    public void Validate_WithAllIdsEmpty_ShouldHaveValidationErrors()
    {
        var command = new DeleteEmployeeSalaryProfileCommand(Guid.Empty, Guid.Empty, Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.EmployeeId);
        result.ShouldHaveValidationErrorFor(x => x.EmployeeSalaryProfileId);
    }
}
