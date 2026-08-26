namespace Application.Tests.Validation;

public class EmployeeBankAccountValidatorTests
{
    private readonly EmployeeBankAccountValidator _validator = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

    [Fact]
    public void Validate_WithValidDto_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildBankAccountDto();

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNullTitle_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildBankAccountDto() with { Title = null };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildBankAccountDto() with { Id = Guid.Empty };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_WithTitleExactly100Characters_ShouldNotHaveAnyErrors()
    {
        var dto = _employeeBuilder.BuildBankAccountDto() with { Title = new string('a', 100) };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithTitleMoreThan100Characters_ShouldHaveValidationError()
    {
        var dto = _employeeBuilder.BuildBankAccountDto() with { Title = new string('a', 101) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    [InlineData("123456789012345678901234")]
    [InlineData("IR123")]
    [InlineData("IR۱۲۳۴۵۶۷۸۹۰۱۲۳۴۵۶۷۸۹۰۱۲")]
    public void Validate_WithInvalidIban_ShouldHaveValidationError(string? iban)
    {
        var dto = _employeeBuilder.BuildBankAccountDto() with { Iban = iban! };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Iban);
    }
}
