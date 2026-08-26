namespace Application.Tests.Features.Employees.Command.UpdateEmployee;

public class UpdateEmployeeCommandValidatorTests
{
    private readonly UpdateEmployeeCommandValidator _validator = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    private UpdateEmployeeCommand CreateValidCommand(EmployeeDto? employee = null, EmployeeInsuranceDto? insurance = null,
        List<EmployeeBankAccountDto>? bankAccounts = null, Guid? userId = null, Guid? employeeId = null)
    {
        return new UpdateEmployeeCommand(
            userId ?? ValidUserId,
            employeeId ?? ValidEmployeeId,
            employee ?? _employeeBuilder.BuildEmployeeDto(),
            insurance ?? _employeeBuilder.BuildInsuranceDto(),
            bankAccounts ?? [_employeeBuilder.BuildBankAccountDto()]);
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
        var command = new UpdateEmployeeCommand(ValidUserId, ValidEmployeeId, null!, _employeeBuilder.BuildInsuranceDto(),
            [_employeeBuilder.BuildBankAccountDto()]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee);
    }

    [Fact]
    public void Validate_WithNullInsurance_ShouldHaveValidationError()
    {
        var command = new UpdateEmployeeCommand(ValidUserId, ValidEmployeeId, _employeeBuilder.BuildEmployeeDto(), null!,
            [_employeeBuilder.BuildBankAccountDto()]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Insurance);
    }

    [Fact]
    public void Validate_WithNullBankAccounts_ShouldHaveValidationError()
    {
        var command = new UpdateEmployeeCommand(ValidUserId, ValidEmployeeId, _employeeBuilder.BuildEmployeeDto(),
            _employeeBuilder.BuildInsuranceDto(), null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BankAccounts);
    }

    [Fact]
    public void Validate_WithEmptyBankAccounts_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(bankAccounts: []);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BankAccounts)
            .WithErrorMessage("کارمند باید حداقل یک حساب بانکی داشته باشد.");
    }

    [Fact]
    public void Validate_WithInvalidBankAccountIban_ShouldHaveValidationError()
    {
        var bankAccounts = new List<EmployeeBankAccountDto>
        {
            _employeeBuilder.BuildBankAccountDto() with { Iban = "123" }
        };
        var command = CreateValidCommand(bankAccounts: bankAccounts);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("BankAccounts[0].Iban");
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

    [Fact]
    public void Validate_WithInvalidInsuranceCalculationProfile_ShouldHaveValidationError()
    {
        var insurance = _employeeBuilder.BuildInsuranceDto() with { InsuranceCalculationProfile = null };
        var command = CreateValidCommand(insurance: insurance);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Insurance.InsuranceCalculationProfile);
    }
}
