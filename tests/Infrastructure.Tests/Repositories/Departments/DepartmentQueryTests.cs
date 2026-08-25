namespace Infrastructure.Tests.Repositories.Departments;

public class DepartmentQueryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private readonly WorkshopBuilder _workshopBuilder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> CreateUserAsync(AsyncServiceScope scope, string? phoneNumber = "09123456789")
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        var user = new UserBuilder().WithPhoneNumber(phoneNumber).CreateResult().ShouldBeSuccess();
        var result = await userRepository.CreateAsync(user, "Pass123456");
        result.Succeeded.Should().BeTrue();
        return user.Id;
    }

    private async Task<Workshop> CreateWorkshopWithDepartmentsAsync(
        AsyncServiceScope scope,
        Guid userId,
        params (Guid Id, string Name)[] departments)
    {
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        foreach (var (id, name) in departments)
            workshop.CreateDepartment(id, name).ShouldBeSuccess();

        if (departments.Length > 0)
            await repository.UpdateAsync(workshop);

        return workshop;
    }

    #region GetUserDepartmentsAsync

    [Fact]
    public async Task GetUserDepartmentsAsync_WithoutFilters_ShouldReturnAllUserDepartments()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId1 = Guid.NewGuid();
        var departmentId2 = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId1, "دپارتمان تولید"),
            (departmentId2, "دپارتمان انبار"));

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(d => d.Name == "دپارتمان تولید" && d.DepartmentId == departmentId1);
        result.Items.Should().Contain(d => d.Name == "دپارتمان انبار" && d.DepartmentId == departmentId2);
        result.Items.Should().OnlyContain(d => d.WorkshopId == workshop.Id && d.WorkshopName == workshop.Name);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithSearchName_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان تولید"),
            (Guid.NewGuid(), "دپارتمان انبار"));

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination, searchName: "تولید");

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("دپارتمان تولید");
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithWorkshopIdFilter_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان اول"));

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه دوم")
            .WithNationalId("2222222222")
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop2);
        workshop2.CreateDepartment(Guid.NewGuid(), "دپارتمان دوم").ShouldBeSuccess();
        await repository.UpdateAsync(workshop2);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination, workshopId: workshop1.Id);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("دپارتمان اول");
        result.Items.First().WorkshopId.Should().Be(workshop1.Id);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithPagination_ShouldPaginateCorrectly()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departments = Enumerable.Range(1, 5)
            .Select(i => (Guid.NewGuid(), $"دپارتمان شماره {i}"))
            .ToArray();
        await CreateWorkshopWithDepartmentsAsync(scope, userId, departments);

        var pagination = new PaginationDto(1, 2);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithSecondPage_ShouldReturnCorrectItems()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departments = Enumerable.Range(1, 5)
            .Select(i => (Guid.NewGuid(), $"دپارتمان شماره {i}"))
            .ToArray();
        await CreateWorkshopWithDepartmentsAsync(scope, userId, departments);

        var pagination = new PaginationDto(2, 2);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithWrongUserId_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان نمونه"));

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(Guid.NewGuid(), pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithCombinedFilters_ShouldReturnCorrectResults()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان تولید"),
            (Guid.NewGuid(), "دپارتمان انبار"));

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه دوم")
            .WithNationalId("2222222222")
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop2);
        workshop2.CreateDepartment(Guid.NewGuid(), "دپارتمان تولید").ShouldBeSuccess();
        await repository.UpdateAsync(workshop2);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(
            userId, pagination, searchName: "تولید", workshopId: workshop1.Id);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("دپارتمان تولید");
        result.Items.First().WorkshopId.Should().Be(workshop1.Id);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_ShouldReturnZeroForEmployeesCount()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان نمونه"));

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Items.Should().ContainSingle();
        result.Items.First().EmployeesCount.Should().Be(0);
    }

    #endregion

    #region GetUserDepartmentsNameAsync

    [Fact]
    public async Task GetUserDepartmentsNameAsync_WhenDepartmentsExist_ShouldReturnNames()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان اول"),
            (Guid.NewGuid(), "دپارتمان دوم"));

        var result = await query.GetUserDepartmentsNameAsync(userId);

        result.Should().HaveCount(2);
        result.Should().Contain(d => d.DisplayName == "دپارتمان اول");
        result.Should().Contain(d => d.DisplayName == "دپارتمان دوم");
    }

    [Fact]
    public async Task GetUserDepartmentsNameAsync_WithLongName_ShouldTruncateDisplayName()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var maxLength = Core.Domain.Department.MaxDisplayNameLength;
        var longName = new string('ا', maxLength + 20);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), longName));

        var result = await query.GetUserDepartmentsNameAsync(userId);

        result.Should().ContainSingle();
        result.First().DisplayName.Should().EndWith("...");
        result.First().DisplayName.Length.Should().Be(maxLength + 3);
    }

    [Fact]
    public async Task GetUserDepartmentsNameAsync_WithShortName_ShouldNotTruncate()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var shortName = "دپارتمان کوتاه";

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), shortName));

        var result = await query.GetUserDepartmentsNameAsync(userId);

        result.Should().ContainSingle();
        result.First().DisplayName.Should().Be(shortName);
    }

    [Fact]
    public async Task GetUserDepartmentsNameAsync_WithWrongUserId_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان نمونه"));

        var result = await query.GetUserDepartmentsNameAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserDepartmentsNameAsync_WhenNoDepartments_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();

        var result = await query.GetUserDepartmentsNameAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserDepartmentByIdAsync

    [Fact]
    public async Task GetUserDepartmentByIdAsync_WhenDepartmentExists_ShouldReturnDepartment()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId, "دپارتمان تولید"));

        var result = await query.GetUserDepartmentByIdAsync(userId, departmentId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("دپارتمان تولید");
        result.WorkshopId.Should().Be(workshop.Id);
    }

    [Fact]
    public async Task GetUserDepartmentByIdAsync_WhenDepartmentDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var result = await query.GetUserDepartmentByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserDepartmentByIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId, "دپارتمان نمونه"));

        var result = await query.GetUserDepartmentByIdAsync(Guid.NewGuid(), departmentId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserDepartmentByIdAsync_WithWrongDepartmentId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان نمونه"));

        var result = await query.GetUserDepartmentByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region IsExistDepartmentName

    [Fact]
    public async Task IsExistDepartmentName_WhenNameExists_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان تولید"));

        var result = await query.IsExistDepartmentName(userId, "دپارتمان تولید");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenNameDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان تولید"));

        var result = await query.IsExistDepartmentName(userId, "دپارتمان دیگر");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenNoDepartmentsExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();

        var result = await query.IsExistDepartmentName(Guid.NewGuid(), "دپارتمان تولید");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithDifferentCase_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "Department ARIA"));

        var result = await query.IsExistDepartmentName(userId, "department aria");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithLeadingOrTrailingSpaces_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان تولید"));

        var result = await query.IsExistDepartmentName(userId, "   دپارتمان تولید   ");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithExcludeDepartmentId_ShouldExcludeThatDepartment()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId, "دپارتمان تولید"));

        var result = await query.IsExistDepartmentName(userId, "دپارتمان تولید", excludeDepartmentId: departmentId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithExcludeDepartmentIdAndOtherDuplicate_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId1 = Guid.NewGuid();
        var departmentId2 = Guid.NewGuid();
        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId1, "دپارتمان تولید"),
            (departmentId2, "دپارتمان تولید"));

        var result = await query.IsExistDepartmentName(userId, "دپارتمان تولید", excludeDepartmentId: departmentId1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithNullExcludeId_ShouldCheckAllDepartments()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان تولید"));

        var result = await query.IsExistDepartmentName(userId, "دپارتمان تولید", excludeDepartmentId: null);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenNameExistsForAnotherUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();

        var user1Id = await CreateUserAsync(scope);
        var user2Id = await CreateUserAsync(scope, "09123456780");

        await CreateWorkshopWithDepartmentsAsync(scope, user1Id,
            (Guid.NewGuid(), "دپارتمان تولید"));

        var result = await query.IsExistDepartmentName(user2Id, "دپارتمان تولید");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenNameExistsInAnotherWorkshopOfSameUser_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "دپارتمان تولید"));

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه دوم")
            .WithNationalId("2222222222")
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop2);

        var result = await query.IsExistDepartmentName(userId, "دپارتمان تولید");

        result.Should().BeTrue();
    }

    #endregion
}
