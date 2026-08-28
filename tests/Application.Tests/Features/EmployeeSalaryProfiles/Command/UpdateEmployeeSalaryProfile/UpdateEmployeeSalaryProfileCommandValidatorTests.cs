namespace Application.Tests.Features.EmployeeSalaryProfiles.Command.UpdateEmployeeSalaryProfile;

public class UpdateEmployeeSalaryProfileCommandValidatorTests
{
    private readonly UpdateEmployeeSalaryProfileCommandValidator _validator = new();
    private readonly EmployeeSalaryProfileBuilder _builder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();

    private UpdateEmployeeSalaryProfileCommand CreateValidCommand(
        EmployeeSalaryProfileDto? salaryProfile = null,
        Guid? userId = null,
        Guid? employeeId = null,
        Guid? salaryProfileId = null)
    {
        var dto = salaryProfile ?? _builder.BuildDto();

        return new UpdateEmployeeSalaryProfileCommand(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            salaryProfileId ?? ValidSalaryProfileId,
            dto);
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
    public void Validate_WithEmptySalaryProfileId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(salaryProfileId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.EmployeeSalaryProfileId);
    }

    [Fact]
    public void Validate_WithNullSalaryProfile_ShouldHaveValidationError()
    {
        var command = new UpdateEmployeeSalaryProfileCommand(
            ValidUserId,
            ValidEmployeeId,
            ValidSalaryProfileId,
            null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile);
    }

    [Fact]
    public void Validate_WithNullEffectiveFrom_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { EffectiveFrom = null };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.EffectiveFrom);
    }

    [Fact]
    public void Validate_WithNullBaseMonthlySalary_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { BaseMonthlySalary = null };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.BaseMonthlySalary);
    }
}
