namespace Core.Tests.Domain.Employees;

public class ControlBankAccountTests
{
    private readonly EmployeeBuilder _builder = new();

    [Fact]
    public void ReplaceBankAccounts_WithSingleBankAccount_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("حساب حقوق", "IR123456789012345678901234")
        ]);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            employee.BankAccounts.Should().ContainSingle();
            employee.BankAccounts.First().Title.Should().Be("حساب حقوق");
            employee.BankAccounts.First().Iban.Should().Be("123456789012345678901234");
        }
    }

    [Fact]
    public void ReplaceBankAccounts_WithMultipleBankAccounts_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("حساب حقوق", "IR123456789012345678901234", Guid.NewGuid()),
            new EmployeeBankAccountDto("حساب پس‌انداز", "IR999999999999999999999999")
        ]);

        result.ShouldBeSuccess();
        employee.BankAccounts.Should().HaveCount(2);
        employee.BankAccounts.Should().Contain(x => x.Title == "حساب حقوق" && x.Iban == "123456789012345678901234");
        employee.BankAccounts.Should().Contain(x => x.Title == "حساب پس‌انداز" && x.Iban == "999999999999999999999999");
    }

    [Fact]
    public void ReplaceBankAccounts_ShouldReplacePreviousBankAccounts()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("اول", "IR123456789012345678901234", Guid.NewGuid()),
            new EmployeeBankAccountDto("دوم", "IR222222222222222222222222", Guid.NewGuid())
        ]).ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("سوم", "IR333333333333333333333333", Guid.NewGuid())
        ]);

        result.ShouldBeSuccess();
        employee.BankAccounts.Should().ContainSingle();
        employee.BankAccounts.First().Title.Should().Be("سوم");
        employee.BankAccounts.First().Iban.Should().Be("333333333333333333333333");
    }

    [Fact]
    public void ReplaceBankAccounts_WithDuplicateIban_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("اول", "IR123456789012345678901234"),
            new EmployeeBankAccountDto("دوم", "IR123456789012345678901234")
        ]);

        result.ShouldBeFailure("شماره شبا در لیست حساب‌های بانکی تکراری است.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithDuplicateId_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var bankAccountId = Guid.NewGuid();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("اول", "IR123456789012345678901234", bankAccountId),
            new EmployeeBankAccountDto("دوم", "IR999999999999999999999999", bankAccountId)
        ]);

        result.ShouldBeFailure("شناسه حساب بانکی در لیست حساب‌های بانکی تکراری است.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithInvalidIban_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("اول", "123")
        ]);

        result.ShouldBeFailure("شماره شبا باید با IR شروع شود و پس از آن 24 رقم انگلیسی داشته باشد.");
    }

    [Fact]
    public void ReplaceBankAccounts_WhenEmployeeIsTerminated_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("حساب حقوق", "IR123456789012345678901234")
        ]);

        result.ShouldBeFailure("کارمند ترک کار شده است");
    }
}
