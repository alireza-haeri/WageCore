namespace Core.Tests.Domain.Employees;

public class ControlBankAccountTests
{
    private readonly EmployeeBuilder _builder = new();

    [Fact]
    public void ReplaceBankAccounts_WithSingleBankAccount_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR123456789012345678901234")
        ]);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            employee.BankAccounts.Should().ContainSingle();
            employee.BankAccounts.First().BankName.Should().Be("بانک ملی");
            employee.BankAccounts.First().BranchCode.Should().Be("۱۰۲");
            employee.BankAccounts.First().Iban.Should().Be("123456789012345678901234");
        }
    }

    [Fact]
    public void ReplaceBankAccounts_WithBlankOptionalFields_ShouldNormalizeToNull()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("   ", " ", "IR123456789012345678901234")
        ]);

        result.ShouldBeSuccess();
        employee.BankAccounts.Should().ContainSingle();
        employee.BankAccounts.First().BankName.Should().BeNull();
        employee.BankAccounts.First().BranchCode.Should().BeNull();
    }

    [Fact]
    public void ReplaceBankAccounts_WithBankNameMoreThan100Characters_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto(new string('a', 101), "۱۰۲", "IR123456789012345678901234")
        ]);

        result.ShouldBeFailure("نام بانک نمیتواند بیشتر از 100 حرف باشد.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithBranchCodeMoreThan100Characters_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("بانک ملی", new string('a', 101), "IR123456789012345678901234")
        ]);

        result.ShouldBeFailure("کد شعبه نمیتواند بیشتر از 100 حرف باشد.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithMultipleBankAccounts_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR123456789012345678901234", Guid.NewGuid()),
            new EmployeeBankAccountDto("بانک صادرات", "۳۰۳", "IR999999999999999999999999")
        ]);

        result.ShouldBeSuccess();
        employee.BankAccounts.Should().HaveCount(2);
        employee.BankAccounts.Should().Contain(x => x.BankName == "بانک ملی" && x.Iban == "123456789012345678901234");
        employee.BankAccounts.Should().Contain(x => x.BankName == "بانک صادرات" && x.Iban == "999999999999999999999999");
    }

    [Fact]
    public void ReplaceBankAccounts_ShouldReplacePreviousBankAccounts()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("اول", "۱", "IR123456789012345678901234", Guid.NewGuid()),
            new EmployeeBankAccountDto("دوم", "۲", "IR222222222222222222222222", Guid.NewGuid())
        ]).ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("سوم", "۳", "IR333333333333333333333333", Guid.NewGuid())
        ]);

        result.ShouldBeSuccess();
        employee.BankAccounts.Should().ContainSingle();
        employee.BankAccounts.First().BankName.Should().Be("سوم");
        employee.BankAccounts.First().BranchCode.Should().Be("۳");
        employee.BankAccounts.First().Iban.Should().Be("333333333333333333333333");
    }

    [Fact]
    public void ReplaceBankAccounts_WithDuplicateIban_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("اول", "۱", "IR123456789012345678901234"),
            new EmployeeBankAccountDto("دوم", "۲", "IR123456789012345678901234")
        ]);

        result.ShouldBeFailure("شماره شبا در لیست حساب‌های بانکی تکراری است.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithDuplicateId_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var bankAccountId = Guid.NewGuid();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("اول", "۱", "IR123456789012345678901234", bankAccountId),
            new EmployeeBankAccountDto("دوم", "۲", "IR999999999999999999999999", bankAccountId)
        ]);

        result.ShouldBeFailure("شناسه حساب بانکی در لیست حساب‌های بانکی تکراری است.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithInvalidIban_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("اول", "۱", "123")
        ]);

        result.ShouldBeFailure("شماره شبا باید با IR شروع شود و پس از آن 24 رقم انگلیسی داشته باشد.");
    }

    [Fact]
    public void ReplaceBankAccounts_WhenEmployeeIsTerminated_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR123456789012345678901234")
        ]);

        result.ShouldBeFailure("کارمند ترک کار شده است");
    }

    [Fact]
    public void ReplaceBankAccounts_WithEmptyList_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([]);

        result.ShouldBeFailure("کارمند باید حداقل یک حساب بانکی داشته باشد.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithNull_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts(null);

        result.ShouldBeFailure("اطلاعات حساب‌های بانکی نمیتواند خالی باشد.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithEmptyList_ShouldNotRemovePreviousBankAccounts()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR123456789012345678901234")
        ]).ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([]);

        result.ShouldBeFailure("کارمند باید حداقل یک حساب بانکی داشته باشد.");
        using (new AssertionScope())
        {
            employee.BankAccounts.Should().ContainSingle();
            employee.BankAccounts.First().Iban.Should().Be("123456789012345678901234");
        }
    }

    [Fact]
    public void ReplaceBankAccounts_WithEmptyList_WhenEmployeeIsTerminated_ShouldFailWithTerminationMessage()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([]);

        result.ShouldBeFailure("کارمند ترک کار شده است");
    }
}
