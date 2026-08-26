namespace Application.Tests.Features.Employees.Command.CreateEmployee;

public class CreateEmployeeCommandValidatorTests
{
    private readonly CreateEmployeeCommandValidator _validator = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();

    private CreateEmployeeCommand CreateValidCommand(EmployeeDto? employee = null, EmployeeInsuranceDto? insurance = null,
        List<EmployeeBankAccountDto>? bankAccounts = null, Guid? userId = null, Guid? workshopId = null)
    {
        var employeeDto = employee ?? _employeeBuilder.BuildEmployeeDto();
        var insuranceDto = insurance ?? _employeeBuilder.BuildInsuranceDto();
        var bankAccountDtos = bankAccounts ?? [_employeeBuilder.BuildBankAccountDto()];

        return new CreateEmployeeCommand(
            userId ?? ValidUserId,
            workshopId ?? ValidWorkshopId,
            employeeDto,
            insuranceDto,
            bankAccountDtos);
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
    public void Validate_WithEmptyWorkshopId_ShouldHaveValidationError()
    {
        var command = CreateValidCommand(workshopId: Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.WorkshopId);
    }

    [Fact]
    public void Validate_WithNullEmployee_ShouldHaveValidationError()
    {
        var command = new CreateEmployeeCommand(ValidUserId, ValidWorkshopId, null!, _employeeBuilder.BuildInsuranceDto(),
            [_employeeBuilder.BuildBankAccountDto()]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee);
    }

    [Fact]
    public void Validate_WithNullInsurance_ShouldHaveValidationError()
    {
        var command = new CreateEmployeeCommand(ValidUserId, ValidWorkshopId, _employeeBuilder.BuildEmployeeDto(), null!,
            [_employeeBuilder.BuildBankAccountDto()]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Insurance);
    }

    [Fact]
    public void Validate_WithNullBankAccounts_ShouldHaveValidationError()
    {
        var command = new CreateEmployeeCommand(ValidUserId, ValidWorkshopId, _employeeBuilder.BuildEmployeeDto(),
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
    public void Validate_WithEmptyDepartmentId_ShouldHaveValidationError()
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with { DepartmentId = Guid.Empty };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.DepartmentId);
    }

    [Theory]
    [InlineData("A-100")]
    [InlineData("۱۲۳")]
    public void Validate_WithInvalidPersonalCode_ShouldHaveValidationError(string personalCode)
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with { PersonalCode = personalCode };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.PersonalCode);
    }

    [Theory]
    [InlineData("عل")]
    public void Validate_WithFullNameLessThan3Characters_ShouldHaveValidationError(string fullName)
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with { FullName = fullName };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.FullName);
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("abcdefghij")]
    [InlineData("۱۲۳۴۵۶۷۸۹۰")]
    public void Validate_WithInvalidNationalCode_ShouldHaveValidationError(string nationalCode)
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with { NationalCode = nationalCode };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.NationalCode);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(21)]
    public void Validate_WithChildrenCountOutOfRange_ShouldHaveValidationError(int childrenCount)
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with { ChildrenCount = childrenCount };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.ChildrenCount);
    }

    [Fact]
    public void Validate_WithHireDateInFuture_ShouldHaveValidationError()
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with
        {
            HireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1))
        };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.HireDate);
    }

    [Theory]
    [MemberData(nameof(PhoneNumberTestData.InvalidLengthPhoneNumbers), MemberType = typeof(PhoneNumberTestData))]
    [MemberData(nameof(PhoneNumberTestData.NoneEnglishDigitsOrInvalidCharacters), MemberType = typeof(PhoneNumberTestData))]
    public void Validate_WithInvalidPhoneNumber_ShouldHaveValidationError(string phoneNumber)
    {
        var employee = _employeeBuilder.BuildEmployeeDto() with { PhoneNumber = phoneNumber };
        var command = CreateValidCommand(employee: employee);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Employee.PhoneNumber);
    }

    [Fact]
    public void Validate_WithInsuranceNumberMoreThan20Characters_ShouldHaveValidationError()
    {
        var insurance = _employeeBuilder.BuildInsuranceDto() with { InsuranceNumber = new string('a', 21) };
        var command = CreateValidCommand(insurance: insurance);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Insurance.InsuranceNumber);
    }

    [Fact]
    public void Validate_WithNullInsuranceCalculationProfile_ShouldHaveValidationError()
    {
        var insurance = _employeeBuilder.BuildInsuranceDto() with { InsuranceCalculationProfile = null };
        var command = CreateValidCommand(insurance: insurance);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Insurance.InsuranceCalculationProfile);
    }
}
