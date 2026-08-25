namespace Core.Tests.Domain.Departments;

public class UpdateDepartmentTests
{
    private readonly DepartmentBuilder _builder = new();

    [Fact]
    public void Update_WithValidData_ShouldReturnSuccess()
    {
        var department = _builder.CreateResult().ShouldBeSuccess();

        var newName = "بخش جدید";

        var result = department.Update(newName);

        result.ShouldBeSuccess();
        department.Name.Should().Be(newName);
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Update_WithNullOrWhiteSpaceName_ShouldFail(string? name)
    {
        var department = _builder.CreateResult().ShouldBeSuccess();

        var result = department.Update(name!);

        result.ShouldBeFailure();
    }

    [Theory]
    [InlineData("ا")]
    [InlineData("آ")]
    public void Update_WithNameLessThan2Characters_ShouldFail(string name)
    {
        var department = _builder.CreateResult().ShouldBeSuccess();

        var result = department.Update(name);

        result.ShouldBeFailure();
    }

    [Fact]
    public void Update_WithNameMoreThan100Characters_ShouldFail()
    {
        var department = _builder.CreateResult().ShouldBeSuccess();
        var name = new string('a', 101);

        var result = department.Update(name);

        result.ShouldBeFailure();
    }

    [Fact]
    public void Update_WithNameExactly2Characters_ShouldReturnSuccess()
    {
        var department = _builder.CreateResult().ShouldBeSuccess();

        var result = department.Update("اب");

        result.ShouldBeSuccess();
        department.Name.Should().Be("اب");
    }

    [Fact]
    public void Update_WithNameExactly100Characters_ShouldReturnSuccess()
    {
        var department = _builder.CreateResult().ShouldBeSuccess();
        var name = new string('a', 100);

        var result = department.Update(name);

        result.ShouldBeSuccess();
        department.Name.Should().Be(name);
    }
}
