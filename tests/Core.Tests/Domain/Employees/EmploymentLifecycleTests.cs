namespace Core.Tests.Domain.Employees;

public class EmploymentLifecycleTests
{
    private readonly EmployeeBuilder _builder = new();

    [Fact]
    public void Terminate_WithValidData_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var terminationDate = DateOnly.FromDateTime(DateTime.Now);

        var result = employee.Terminate(terminationDate);

        result.ShouldBeSuccess();
        employee.TerminationDate.Should().Be(terminationDate);
    }

    [Fact]
    public void Terminate_WithDateBeforeHireDate_ShouldFail()
    {
        var employee = _builder.WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-5))).CreateResult().ShouldBeSuccess();

        var result = employee.Terminate(DateOnly.FromDateTime(DateTime.Now.AddDays(-6)));

        result.ShouldBeFailure("تاریخ ترک کار نباید قبل از تاریخ استخدام باشد.");
    }

    [Fact]
    public void Rehire_WithValidData_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.Terminate(DateOnly.FromDateTime(DateTime.Now.AddDays(-1))).ShouldBeSuccess();
        var newDepartmentId = Guid.NewGuid();
        var newHireDate = DateOnly.FromDateTime(DateTime.Now);
        var rehireDto = new EmployeeRehireDto(
            newDepartmentId,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
            newHireDate);

        var result = employee.Rehire(rehireDto);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            employee.DepartmentId.Should().Be(newDepartmentId);
            employee.HireDate.Should().Be(newHireDate);
            employee.TerminationDate.Should().BeNull();
        }
    }

    [Fact]
    public void Rehire_WithDateNotAfterTerminationDate_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var terminationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
        employee.Terminate(terminationDate).ShouldBeSuccess();
        var rehireDto = new EmployeeRehireDto(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
            terminationDate);

        var result = employee.Rehire(rehireDto);

        result.ShouldBeFailure("تاریخ استخدام مجدد باید بعد از تاریخ ترک کار باشد.");
    }

    [Fact]
    public void Rehire_WhenEmployeeIsActive_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var rehireDto = new EmployeeRehireDto(
            Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Now.AddDays(-30)),
            DateOnly.FromDateTime(DateTime.Now));

        var result = employee.Rehire(rehireDto);

        result.ShouldBeFailure("تنها کارمند ترک کار شده را میتوان دوباره استخدام کرد.");
    }

    [Fact]
    public void EnsureEmployedDuring_WhenHiredAfterThePeriod_ShouldFail()
    {
        var periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
        var employee = _builder
            .WithHireDate(periodStart.AddDays(25))
            .CreateResult()
            .ShouldBeSuccess();

        var result = employee.EnsureEmployedDuring(periodStart, periodStart.AddDays(24));

        result.ShouldBeFailure("کارمند در این بازه استخدام نشده بود.");
    }

    [Fact]
    public void EnsureEmployedDuring_WhenTerminatedBeforeThePeriod_ShouldFail()
    {
        var periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
        var employee = _builder
            .WithWorkshopRegistrationDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-90)))
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-60)))
            .CreateResult()
            .ShouldBeSuccess();
        employee.Terminate(periodStart.AddDays(-3)).ShouldBeSuccess();

        var result = employee.EnsureEmployedDuring(periodStart, periodStart.AddDays(24));

        result.ShouldBeFailure("کارمند قبل از این بازه ترک کار کرده است.");
    }

    [Fact]
    public void EnsureEmployedDuring_WhenHiredMidPeriod_ShouldSucceed()
    {
        var periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
        var employee = _builder
            .WithHireDate(periodStart.AddDays(10))
            .CreateResult()
            .ShouldBeSuccess();

        employee.EnsureEmployedDuring(periodStart, periodStart.AddDays(24)).ShouldBeSuccess();
    }

    [Fact]
    public void EnsureEmployedDuring_WhenTerminatedMidPeriod_ShouldSucceed()
    {
        var periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
        var employee = _builder
            .WithWorkshopRegistrationDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-90)))
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-60)))
            .CreateResult()
            .ShouldBeSuccess();
        employee.Terminate(periodStart.AddDays(10)).ShouldBeSuccess();

        employee.EnsureEmployedDuring(periodStart, periodStart.AddDays(24)).ShouldBeSuccess();
    }

    [Fact]
    public void EnsureEmployedDuring_WhenTerminatedOnThePeriodStart_ShouldSucceed()
    {
        var periodStart = DateOnly.FromDateTime(DateTime.Now.AddDays(-25));
        var employee = _builder
            .WithWorkshopRegistrationDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-90)))
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-60)))
            .CreateResult()
            .ShouldBeSuccess();
        employee.Terminate(periodStart).ShouldBeSuccess();

        employee.EnsureEmployedDuring(periodStart, periodStart.AddDays(24)).ShouldBeSuccess();
    }
}
