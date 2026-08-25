namespace Core.Tests.Domain.Workshops;

public class ControlDepartmentTests
{
    private readonly WorkshopBuilder _builder = new();

    [Fact]
    public void CreateDepartment_WithValidData_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.CreateDepartment("دپارتمان نمونه");

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.WorkshopId.Should().Be(workshop.Id);
            response.Name.Should().Be("دپارتمان نمونه");
            workshop.Departments.Should().Contain(response);
        }
    }

    [Fact]
    public void CreateDepartment_WithAllValidFields_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var departmentId = Guid.NewGuid();
        var name = "دپارتمان نساجی";

        var result = workshop.CreateDepartment(departmentId, name);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(departmentId);
            response.WorkshopId.Should().Be(workshop.Id);
            response.Name.Should().Be(name);
            workshop.Departments.Should().Contain(response);
        }
    }

    [Fact]
    public void CreateDepartment_WithEmptyGuid_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.CreateDepartment(Guid.Empty, "دپارتمان نمونه");

        result.ShouldBeFailure();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void CreateDepartment_WithNullOrWhiteSpaceName_ShouldFail(string? name)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.CreateDepartment(name!);

        result.ShouldBeFailure();
    }

    [Theory]
    [InlineData("ا")]
    [InlineData("آ")]
    public void CreateDepartment_WithNameLessThan2Characters_ShouldFail(string name)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.CreateDepartment(name);

        result.ShouldBeFailure();
    }

    [Fact]
    public void CreateDepartment_WithNameMoreThan100Characters_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var name = new string('a', 101);

        var result = workshop.CreateDepartment(name);

        result.ShouldBeFailure();
    }

    [Fact]
    public void CreateDepartment_WithNameExactly2Characters_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.CreateDepartment("اب");

        result.ShouldBeSuccess();
    }

    [Fact]
    public void CreateDepartment_WithNameExactly100Characters_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var name = new string('a', 100);

        var result = workshop.CreateDepartment(name);

        result.ShouldBeSuccess();
    }

    [Fact]
    public void UpdateDepartment_WithValidData_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var department = workshop.CreateDepartment("دپارتمان نمونه").ShouldBeSuccess();

        var result = workshop.UpdateDepartment(department.Id, "دپارتمان جدید");

        result.ShouldBeSuccess();
        department.Name.Should().Be("دپارتمان جدید");
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void UpdateDepartment_WithNullOrWhiteSpaceName_ShouldFail(string? name)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var department = workshop.CreateDepartment("دپارتمان نمونه").ShouldBeSuccess();

        var result = workshop.UpdateDepartment(department.Id, name!);

        result.ShouldBeFailure();
    }

    [Theory]
    [InlineData("ا")]
    [InlineData("آ")]
    public void UpdateDepartment_WithNameLessThan2Characters_ShouldFail(string name)
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var department = workshop.CreateDepartment("دپارتمان نمونه").ShouldBeSuccess();

        var result = workshop.UpdateDepartment(department.Id, name);

        result.ShouldBeFailure();
    }

    [Fact]
    public void UpdateDepartment_WithNameMoreThan100Characters_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var department = workshop.CreateDepartment("دپارتمان نمونه").ShouldBeSuccess();
        var name = new string('a', 101);

        var result = workshop.UpdateDepartment(department.Id, name);

        result.ShouldBeFailure();
    }

    [Fact]
    public void UpdateDepartment_WithNameExactly2Characters_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var department = workshop.CreateDepartment("دپارتمان نمونه").ShouldBeSuccess();

        var result = workshop.UpdateDepartment(department.Id, "اب");

        result.ShouldBeSuccess();
        department.Name.Should().Be("اب");
    }

    [Fact]
    public void UpdateDepartment_WithNameExactly100Characters_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var department = workshop.CreateDepartment("دپارتمان نمونه").ShouldBeSuccess();
        var name = new string('a', 100);

        var result = workshop.UpdateDepartment(department.Id, name);

        result.ShouldBeSuccess();
        department.Name.Should().Be(name);
    }

    [Fact]
    public void UpdateDepartment_WithNotFoundDepartment_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.UpdateDepartment(Guid.NewGuid(), "دپارتمان جدید");

        result.ShouldBeFailure();
    }

    [Fact]
    public void DeleteDepartment_WithValidData_ShouldReturnSuccess()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var department = workshop.CreateDepartment("دپارتمان نمونه").ShouldBeSuccess();

        var result = workshop.DeleteDepartment(department.Id);

        result.ShouldBeSuccess();
        workshop.Departments.Should().NotContain(department);
    }

    [Fact]
    public void DeleteDepartment_WithNotFoundDepartment_ShouldFail()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();

        var result = workshop.DeleteDepartment(Guid.NewGuid());

        result.ShouldBeFailure();
    }

    [Fact]
    public void DeleteDepartment_ShouldOnlyRemoveTargetDepartment()
    {
        var workshop = _builder.CreateResult().ShouldBeSuccess();
        var department1 = workshop.CreateDepartment("دپارتمان اول").ShouldBeSuccess();
        var department2 = workshop.CreateDepartment("دپارتمان دوم").ShouldBeSuccess();

        var result = workshop.DeleteDepartment(department1.Id);

        result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            workshop.Departments.Should().NotContain(department1);
            workshop.Departments.Should().Contain(department2);
            workshop.Departments.Should().HaveCount(1);
        }
    }
}
