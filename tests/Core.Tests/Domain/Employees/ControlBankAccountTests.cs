namespace Core.Tests.Domain.Employees;

public class ControlBankAccountTests
{
    private readonly EmployeeBuilder _builder = new();

    [Fact]
    public void CreateBankAccount_WithValidData_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var bankAccountDto = new EmployeeBankAccountDto("حساب حقوق", "IR123456789012345678901234");

        var result = employee.CreateBankAccount(bankAccountDto);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.Title.Should().Be("حساب حقوق");
            response.Iban.Should().Be("123456789012345678901234");
            employee.BankAccounts.Should().Contain(response);
        }
    }

    [Fact]
    public void CreateBankAccount_WithDuplicateIban_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.CreateBankAccount(new EmployeeBankAccountDto("اول", "IR123456789012345678901234")).ShouldBeSuccess();

        var result = employee.CreateBankAccount(new EmployeeBankAccountDto("دوم", "IR123456789012345678901234"));

        result.ShouldBeFailure("شماره شبا برای این کارمند تکراری است.");
    }

    [Fact]
    public void ReplaceBankAccounts_WithValidData_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.CreateBankAccount(Guid.NewGuid(), new EmployeeBankAccountDto("اول", "IR123456789012345678901234"))
            .ShouldBeSuccess();

        var result = employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("حساب حقوق", "IR999999999999999999999999", Guid.NewGuid()),
            new EmployeeBankAccountDto("حساب پس‌انداز", "IR888888888888888888888888")
        ]);

        result.ShouldBeSuccess();
        employee.BankAccounts.Should().HaveCount(2);
        employee.BankAccounts.Should().Contain(x => x.Title == "حساب حقوق" && x.Iban == "999999999999999999999999");
        employee.BankAccounts.Should().Contain(x => x.Title == "حساب پس‌انداز" && x.Iban == "888888888888888888888888");
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
    public void UpdateBankAccount_WithValidData_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var bankAccount = employee.CreateBankAccount(new EmployeeBankAccountDto("حساب اول", "IR123456789012345678901234")).ShouldBeSuccess();

        var result = employee.UpdateBankAccount(bankAccount.Id, new EmployeeBankAccountDto("حساب دوم", "IR999999999999999999999999"));

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            bankAccount.Title.Should().Be("حساب دوم");
            bankAccount.Iban.Should().Be("999999999999999999999999");
        }
    }

    [Fact]
    public void DeleteBankAccount_WithValidData_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var bankAccount = employee.CreateBankAccount(new EmployeeBankAccountDto("حساب اول", "IR123456789012345678901234")).ShouldBeSuccess();

        var result = employee.DeleteBankAccount(bankAccount.Id);

        result.ShouldBeSuccess();
        employee.BankAccounts.Should().NotContain(bankAccount);
    }

    [Fact]
    public void CreateBankAccount_WhenEmployeeIsTerminated_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();

        var result = employee.CreateBankAccount(new EmployeeBankAccountDto("حساب حقوق", "IR123456789012345678901234"));

        result.ShouldBeFailure("کارمند ترک کار شده است");
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
