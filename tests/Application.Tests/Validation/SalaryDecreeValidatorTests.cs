namespace Application.Tests.Validation;

public class SalaryDecreeValidatorTests
{
    private readonly SalaryDecreeValidator _validator = new();
    private readonly SalaryDecreeBuilder _builder = new();

    [Fact]
    public void Validate_WithValidDto_ShouldNotHaveAnyErrors()
    {
        var dto = _builder.BuildDto();

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
    public void Validate_WithNullBaseDailySalary_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { BaseDailySalary = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BaseDailySalary);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void Validate_WithBaseDailySalaryLessThanOrEqualToZero_ShouldHaveValidationError(decimal amount)
    {
        var dto = _builder.BuildDto() with { BaseDailySalary = amount };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BaseDailySalary);
    }

    [Fact]
    public void Validate_WithNullContractType_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { ContractType = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ContractType);
    }

    [Fact]
    public void Validate_WithInvalidContractType_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { ContractType = (ContractType)999 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ContractType);
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

    [Fact]
    public void Validate_WithNullMaritalStatus_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { MaritalStatus = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MaritalStatus);
    }

    [Fact]
    public void Validate_WithInvalidMaritalStatus_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { MaritalStatus = (EmployeeMaritalStatus)999 };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.MaritalStatus);
    }

    [Fact]
    public void Validate_WithNullChildrenCount_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { ChildrenCount = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ChildrenCount);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Validate_WithChildrenCountOutOfRange_ShouldHaveValidationError(int childrenCount)
    {
        var dto = _builder.BuildDto() with { ChildrenCount = childrenCount };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ChildrenCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    public void Validate_WithChildrenCountBoundary_ShouldNotHaveAnyErrors(int childrenCount)
    {
        var dto = _builder
            .WithMaritalStatus(EmployeeMaritalStatus.Married)
            .BuildDto() with { ChildrenCount = childrenCount };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ChildrenCount);
    }

    [Fact]
    public void Validate_WithSingleMaritalStatusAndChildrenCountMoreThanZero_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with
        {
            MaritalStatus = EmployeeMaritalStatus.Single,
            ChildrenCount = 1
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ChildrenCount);
    }

    [Fact]
    public void Validate_WithNullIsTaxSubject_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { IsTaxSubject = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.IsTaxSubject);
    }

    [Fact]
    public void Validate_WithNullInsurance_ShouldHaveValidationError()
    {
        var dto = _builder.BuildDto() with { Insurance = null };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Insurance);
    }
}
