namespace Application.Tests.Features.SalaryDecrees.Command.UpdateSalaryDecree;

public class UpdateSalaryDecreeCommandValidatorTests
{
    private readonly UpdateSalaryDecreeCommandValidator _validator = new();
    private readonly SalaryDecreeBuilder _builder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();

    private UpdateSalaryDecreeCommand CreateValidCommand(
        SalaryDecreeDto? salaryProfile = null,
        Guid? userId = null,
        Guid? employeeId = null,
        Guid? salaryProfileId = null)
    {
        var dto = salaryProfile ?? _builder.BuildDto();

        return new UpdateSalaryDecreeCommand(
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

        result.ShouldHaveValidationErrorFor(x => x.SalaryDecreeId);
    }

    [Fact]
    public void Validate_WithNullSalaryProfile_ShouldHaveValidationError()
    {
        var command = new UpdateSalaryDecreeCommand(
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
    public void Validate_WithNullBaseDailySalary_ShouldHaveValidationError()
    {
        var salaryProfile = _builder.BuildDto() with { BaseDailySalary = null };
        var command = CreateValidCommand(salaryProfile);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SalaryProfile.BaseDailySalary);
    }
}
