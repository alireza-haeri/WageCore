namespace Core.Tests.Domain.Employees;

public class UpdateEmployeeTests
{
    private readonly EmployeeBuilder _builder = new();

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var workshopId = employee.WorkshopId;
        var workshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));
        var newDepartmentId = Guid.NewGuid();
        var employeeDto = new EmployeeDto(
            newDepartmentId,
            "EMP777",
            "کارمند جدید",
            "0987654321",
            "محمود",
            EmployeeGender.Woman,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
            "09987654321",
            "سرپرست",
            Region.Normal);

        var result = employee.Update(employeeDto, workshopRegistrationDate);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            employee.DepartmentId.Should().Be(newDepartmentId);
            employee.WorkshopId.Should().Be(workshopId);
            employee.PersonalCode.Should().Be("EMP777");
            employee.FullName.Should().Be("کارمند جدید");
            employee.NationalCode.Should().Be("0987654321");
            employee.FatherName.Should().Be("محمود");
            employee.Gender.Should().Be(EmployeeGender.Woman);
            employee.PhoneNumber.Should().Be("09987654321");
            employee.JobTitle.Should().Be("سرپرست");
            employee.Region.Should().Be(Region.Normal);
        }
    }

    [Fact]
    public void Update_ShouldNotChangeWorkshopId()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var workshopId = employee.WorkshopId;
        var employeeDto = new EmployeeDto(
            Guid.NewGuid(),
            employee.PersonalCode,
            employee.FullName,
            employee.NationalCode,
            employee.FatherName,
            employee.Gender,
            employee.HireDate,
            employee.PhoneNumber,
            employee.JobTitle,
            employee.Region);

        var result = employee.Update(employeeDto, DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));

        result.ShouldBeSuccess();
        employee.WorkshopId.Should().Be(workshopId);
    }

    [Fact]
    public void Update_WithDuplicatePersonalCode_WhenCodeChanges_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var employeeDto = new EmployeeDto(
            employee.DepartmentId,
            "NEW100",
            employee.FullName,
            employee.NationalCode,
            employee.FatherName,
            employee.Gender,
            employee.HireDate,
            employee.PhoneNumber,
            employee.JobTitle,
            employee.Region);

        var result = employee.Update(employeeDto, DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), false, true);

        result.ShouldBeFailure("کد پرسنلی در بین کارکنان این کاربر تکراری است.");
    }

    [Fact]
    public void Update_WithDuplicateNationalCode_WhenCodeChanges_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        var employeeDto = new EmployeeDto(
            employee.DepartmentId,
            employee.PersonalCode,
            employee.FullName,
            "1111111111",
            employee.FatherName,
            employee.Gender,
            employee.HireDate,
            employee.PhoneNumber,
            employee.JobTitle,
            employee.Region);

        var result = employee.Update(employeeDto, DateOnly.FromDateTime(DateTime.Now.AddDays(-30)), true, false);

        result.ShouldBeFailure("کد ملی در بین کارکنان این کاربر تکراری است.");
    }

    [Fact]
    public void Update_WithTerminationDate_ShouldFail()
    {
        var employee = _builder.CreateResult().ShouldBeSuccess();
        employee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();
        var employeeDto = new EmployeeDto(
            employee.DepartmentId,
            employee.PersonalCode,
            employee.FullName,
            employee.NationalCode,
            employee.FatherName,
            employee.Gender,
            employee.HireDate,
            employee.PhoneNumber,
            employee.JobTitle,
            employee.Region);

        var result = employee.Update(employeeDto, DateOnly.FromDateTime(DateTime.Now.AddDays(-30)));

        result.ShouldBeFailure("کارمند ترک کار شده است");
    }
}
