namespace Application.Tests.Features.EmployeeSalaryProfiles.Command.CreateEmployeeSalaryProfile;

public class CreateEmployeeSalaryProfileCommandValidatorTests
{
    private readonly CreateEmployeeSalaryProfileCommandValidator _validator = new();
    private readonly EmployeeSalaryProfileBuilder _builder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    private CreateEmployeeSalaryProfileCommand CreateValidCommand(
        EmployeeSalaryProfileDto? salaryProfile = null,
        Guid? userId = null,
        Guid? employeeId = null)
    {
        var dto = salaryProfile ?? _builder.BuildDto();

        return new CreateEmployeeSalaryProfileCommand(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
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
    public void Validate_WithNullSalaryProfile_ShouldHaveValidationError()
    {
        var command = new CreateEmployeeSalaryProfileCommand(ValidUserId, ValidEmployeeId, null!);

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

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithBaseMonthlySalaryLessThanOrEqualToZero_ShouldHaveValidationError(decimal amount)
    {
        var salaryProfile = _builder.BuildDto() with { BaseMonthlySalary = amount };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.BaseMonthlySalary);
    }

    [Fact]
    public void Validate_WithAutomaticModeAndNullCalculationMethod_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with
        {
            SeniorityBaseApplicationMode = SeniorityBaseApplicationMode.Automatic,
            SeniorityBaseCalculationMethod = null
        };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.SeniorityBaseCalculationMethod);
    }

    [Fact]
    public void Validate_WithManualModeAndFilledCalculationMethod_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with
        {
            SeniorityBaseApplicationMode = SeniorityBaseApplicationMode.Manual,
            SeniorityBaseCalculationMethod = SeniorityBaseCalculationMethod.Daily
        };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.SeniorityBaseCalculationMethod);
    }

    [Fact]
    public void Validate_WithNullYearEndSeniorityMode_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { YearEndSeniorityMode = null };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.YearEndSeniorityMode);
    }

    [Fact]
    public void Validate_WithNullShiftType_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { ShiftType = null };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.ShiftType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithAttractionAllowanceLessThanOrEqualToZero_ShouldHaveValidationError(decimal amount)
    {
        var salaryProfile = _builder.BuildDto() with { AttractionAllowance = amount };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.AttractionAllowance);
    }
}
