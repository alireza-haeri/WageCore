namespace Core.Tests.Domain.Departments;

public class CreateDepartmentTests
{
    private readonly DepartmentBuilder _builder = new();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = _builder.CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().NotBeEmpty();
            response.WorkshopId.Should().NotBeEmpty();
            response.Name.Should().Be("دپارتمان نمونه");
        }
    }

    [Fact]
    public void Create_WithAllValidFields_ShouldReturnSuccess()
    {
        var id = Guid.NewGuid();
        var workshopId = Guid.NewGuid();
        var name = "دپارتمان نساجی";

        var result = _builder
            .WithId(id)
            .WithWorkshopId(workshopId)
            .WithName(name)
            .CreateResult();

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.Id.Should().Be(id);
            response.WorkshopId.Should().Be(workshopId);
            response.Name.Should().Be(name);
        }
    }

    [Fact]
    public void Create_WithEmptyGuid_ShouldFail()
    {
        var result = _builder.WithId(Guid.Empty).CreateResult();
        result.ShouldBeFailure();
    }

    [Fact]
    public void Create_WithEmptyWorkshopId_ShouldFail()
    {
        var result = _builder.WithWorkshopId(Guid.Empty).CreateResult();
        result.ShouldBeFailure();
    }

    [Theory]
    [MemberData(nameof(StringTestData.NullOrWhiteSpace), MemberType = typeof(StringTestData))]
    public void Create_WithNullOrWhiteSpaceName_ShouldFail(string? name)
    {
        var result = _builder.WithName(name!).CreateResult();
        result.ShouldBeFailure();
    }

    [Theory]
    [InlineData("ا")]
    [InlineData("آ")]
    public void Create_WithNameLessThan2Characters_ShouldFail(string name)
    {
        var result = _builder.WithName(name).CreateResult();
        result.ShouldBeFailure();
    }

    [Fact]
    public void Create_WithNameMoreThan100Characters_ShouldFail()
    {
        var name = new string('a', 101);
        var result = _builder.WithName(name).CreateResult();
        result.ShouldBeFailure();
    }

    [Fact]
    public void Create_WithNameExactly2Characters_ShouldReturnSuccess()
    {
        var result = _builder.WithName("اب").CreateResult();
        result.ShouldBeSuccess();
    }

    [Fact]
    public void Create_WithNameExactly100Characters_ShouldReturnSuccess()
    {
        var name = new string('a', 100);
        var result = _builder.WithName(name).CreateResult();
        result.ShouldBeSuccess();
    }
}
