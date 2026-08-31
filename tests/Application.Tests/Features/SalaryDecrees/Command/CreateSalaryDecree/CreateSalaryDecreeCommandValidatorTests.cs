namespace Application.Tests.Features.SalaryDecrees.Command.CreateSalaryDecree;

public class CreateSalaryDecreeCommandValidatorTests
{
    private readonly CreateSalaryDecreeCommandValidator _validator = new();
    private readonly SalaryDecreeBuilder _builder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    private CreateSalaryDecreeCommand CreateValidCommand(
        SalaryDecreeDto? salaryProfile = null,
        Guid? userId = null,
        Guid? employeeId = null)
    {
        var dto = salaryProfile ?? _builder.BuildDto();

        return new CreateSalaryDecreeCommand(
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
        var command = new CreateSalaryDecreeCommand(ValidUserId, ValidEmployeeId, null!);

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
    public void Validate_WithNullBaseDailySalary_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { BaseDailySalary = null };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.BaseDailySalary);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithBaseDailySalaryLessThanOrEqualToZero_ShouldHaveValidationError(decimal amount)
    {
        var salaryProfile = _builder.BuildDto() with { BaseDailySalary = amount };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.BaseDailySalary);
    }

    [Fact]
    public void Validate_WithNullShiftType_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { ShiftType = null };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.ShiftType);
    }

    [Fact]
    public void Validate_WithNullContractType_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { ContractType = null };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.ContractType);
    }

    [Fact]
    public void Validate_WithInvalidContractType_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { ContractType = (ContractType)999 };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.ContractType);
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
