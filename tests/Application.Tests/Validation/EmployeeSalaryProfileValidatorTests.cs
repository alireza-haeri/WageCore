namespace Application.Tests.Validation;

public class EmployeeSalaryProfileValidatorTests
{
    private readonly EmployeeSalaryProfileValidator _validator = new();
    private readonly EmployeeSalaryProfileBuilder _builder = new();

    [Fact]
    public void Validate_WithValidDto_ShouldNotHaveAnyErrors()
    {
        var dto = _builder.BuildDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAutomaticModeAndDailyMethod_ShouldNotHaveAnyErrors()
    {
        var dto = _builder
            .WithSeniorityBaseApplicationMode(SeniorityBaseApplicationMode.Automatic)
            .WithSeniorityBaseCalculationMethod(SeniorityBaseCalculationMethod.Daily)
            .BuildDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullEffectiveFrom_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { EffectiveFrom = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.EffectiveFrom);
    }

    [Fact]
    public void Validate_WithNullBaseMonthlySalary_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { BaseMonthlySalary = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BaseMonthlySalary);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Validate_WithBaseMonthlySalaryLessThanOrEqualToZero_ShouldHaveValidationError(decimal amount)
    {
        var dto = _builder.BuildDto() with { BaseMonthlySalary = amount };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BaseMonthlySalary);
    }

    [Fact]
    public void Validate_WithNullSeniorityBaseApplicationMode_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { SeniorityBaseApplicationMode = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SeniorityBaseApplicationMode);
    }

    [Fact]
    public void Validate_WithAutomaticModeAndNullCalculationMethod_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with
        {
            SeniorityBaseApplicationMode = SeniorityBaseApplicationMode.Automatic,
            SeniorityBaseCalculationMethod = null
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SeniorityBaseCalculationMethod)
            .WithErrorMessage("روش محاسبه پایه سنوات در حالت خودکار الزامی است.");
    }

    [Fact]
    public void Validate_WithManualModeAndFilledCalculationMethod_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with
        {
            SeniorityBaseApplicationMode = SeniorityBaseApplicationMode.Manual,
            SeniorityBaseCalculationMethod = SeniorityBaseCalculationMethod.CumulativeAuto
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.SeniorityBaseCalculationMethod)
            .WithErrorMessage("روش محاسبه پایه سنوات در حالت دستی نباید پر شود.");
    }

    [Fact]
    public void Validate_WithNullYearEndSeniorityMode_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { YearEndSeniorityMode = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.YearEndSeniorityMode);
    }

    [Fact]
    public void Validate_WithNullShiftType_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { ShiftType = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ShiftType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithHousingAllowanceLessThanOrEqualToZero_ShouldHaveValidationError(decimal amount)
    {
        var dto = _builder.BuildDto() with { HousingAllowance = amount };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.HousingAllowance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithKaranehAmountNetLessThanOrEqualToZero_ShouldHaveValidationError(decimal amount)
    {
        var dto = _builder.BuildDto() with { KaranehAmountNet = amount };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.KaranehAmountNet);
    }
}
