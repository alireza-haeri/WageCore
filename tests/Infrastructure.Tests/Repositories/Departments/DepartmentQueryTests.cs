namespace Infrastructure.Tests.Repositories.Departments;

public class DepartmentQueryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    private readonly WorkshopBuilder _workshopBuilder = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Core.Domain.Employee> CreateEmployeeAsync(AsyncServiceScope scope, Guid workshopId,
        Guid departmentId, string personalCode, string nationalCode)
    {
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();

        var employee = _employeeBuilder
            .WithId(Guid.NewGuid())
            .WithWorkshopId(workshopId)
            .WithDepartmentId(departmentId)
            .WithWorkshopRegistrationDate(ValidWorkshopRegistrationDate)
            .WithPersonalCode(personalCode)
            .WithNationalCode(nationalCode)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(employee);
        result.Should().Be(employee.Id);

        return employee;
    }

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
            (departmentId1, "بخش تولید"),
            (departmentId2, "بخش انبار"));

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(d => d.Name == "بخش تولید" && d.DepartmentId == departmentId1);
        result.Items.Should().Contain(d => d.Name == "بخش انبار" && d.DepartmentId == departmentId2);
        result.Items.Should().OnlyContain(d => d.WorkshopId == workshop.Id && d.WorkshopName == workshop.Name);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithSearchName_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش تولید"),
            (Guid.NewGuid(), "بخش انبار"));

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination, searchName: "تولید");

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("بخش تولید");
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithWorkshopIdFilter_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش اول"));

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه دوم")
            .WithNationalId("2222222222")
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop2);
        workshop2.CreateDepartment(Guid.NewGuid(), "بخش دوم").ShouldBeSuccess();
        await repository.UpdateAsync(workshop2);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination, workshopId: workshop1.Id);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("بخش اول");
        result.Items.First().WorkshopId.Should().Be(workshop1.Id);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WithPagination_ShouldPaginateCorrectly()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departments = Enumerable.Range(1, 5)
            .Select(i => (Guid.NewGuid(), $"بخش شماره {i}"))
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
            .Select(i => (Guid.NewGuid(), $"بخش شماره {i}"))
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
            (Guid.NewGuid(), "بخش نمونه"));

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
            (Guid.NewGuid(), "بخش تولید"),
            (Guid.NewGuid(), "بخش انبار"));

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه دوم")
            .WithNationalId("2222222222")
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop2);
        workshop2.CreateDepartment(Guid.NewGuid(), "بخش تولید").ShouldBeSuccess();
        await repository.UpdateAsync(workshop2);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(
            userId, pagination, searchName: "تولید", workshopId: workshop1.Id);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("بخش تولید");
        result.Items.First().WorkshopId.Should().Be(workshop1.Id);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WhenDepartmentHasNoEmployees_ShouldReturnZeroForEmployeesCount()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش نمونه"));

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Items.Should().ContainSingle();
        result.Items.First().EmployeesCount.Should().Be(0);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_WhenDepartmentHasEmployees_ShouldReturnEmployeesCount()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId, "بخش تولید"));

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "1234567890");
        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "0987654321");
        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP003", "1111111111");

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Items.Should().ContainSingle();
        result.Items.First().EmployeesCount.Should().Be(3);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_ShouldCountOnlyEmployeesOfEachDepartment()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId1 = Guid.NewGuid();
        var departmentId2 = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId1, "بخش تولید"),
            (departmentId2, "بخش انبار"));

        await CreateEmployeeAsync(scope, workshop.Id, departmentId1, "EMP001", "1234567890");
        await CreateEmployeeAsync(scope, workshop.Id, departmentId1, "EMP002", "0987654321");
        await CreateEmployeeAsync(scope, workshop.Id, departmentId2, "EMP003", "1111111111");

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Items.Should().HaveCount(2);
        result.Items.First(x => x.DepartmentId == departmentId1).EmployeesCount.Should().Be(2);
        result.Items.First(x => x.DepartmentId == departmentId2).EmployeesCount.Should().Be(1);
    }

    [Fact]
    public async Task GetUserDepartmentsAsync_ShouldCountTerminatedEmployeesAsWell()
    {
        await using var scope = fixture.CreateScope();
        var employeeRepository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId, "بخش تولید"));

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "1234567890");
        var terminatedEmployee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "0987654321");

        terminatedEmployee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();
        await employeeRepository.UpdateAsync(terminatedEmployee);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserDepartmentsAsync(userId, pagination);

        result.Items.Should().ContainSingle();
        result.Items.First().EmployeesCount.Should().Be(2);
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
            (Guid.NewGuid(), "بخش اول"),
            (Guid.NewGuid(), "بخش دوم"));

        var result = await query.GetUserDepartmentsNameAsync(userId);

        result.Should().HaveCount(2);
        result.Should().Contain(d => d.DisplayName == "بخش اول");
        result.Should().Contain(d => d.DisplayName == "بخش دوم");
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

        var shortName = "بخش کوتاه";

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
            (Guid.NewGuid(), "بخش نمونه"));

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
            (departmentId, "بخش تولید"));

        var result = await query.GetUserDepartmentByIdAsync(userId, departmentId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("بخش تولید");
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
            (departmentId, "بخش نمونه"));

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
            (Guid.NewGuid(), "بخش نمونه"));

        var result = await query.GetUserDepartmentByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region IsExistDepartmentName

    [Fact]
    public async Task IsExistDepartmentName_WhenNameExistsInWorkshop_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش تولید"));

        var result = await query.IsExistDepartmentName(workshop.Id, "بخش تولید");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenNameDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش تولید"));

        var result = await query.IsExistDepartmentName(workshop.Id, "بخش دیگر");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenWorkshopDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();

        var result = await query.IsExistDepartmentName(Guid.NewGuid(), "بخش تولید");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithDifferentCase_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "Department ARIA"));

        var result = await query.IsExistDepartmentName(workshop.Id, "department aria");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithLeadingOrTrailingSpaces_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش تولید"));

        var result = await query.IsExistDepartmentName(workshop.Id, "   بخش تولید   ");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithExcludeDepartmentId_ShouldExcludeThatDepartment()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId, "بخش تولید"));

        var result = await query.IsExistDepartmentName(workshop.Id, "بخش تولید", excludeDepartmentId: departmentId);

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
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (departmentId1, "بخش تولید"),
            (departmentId2, "بخش تولید"));

        var result = await query.IsExistDepartmentName(workshop.Id, "بخش تولید", excludeDepartmentId: departmentId1);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WithNullExcludeId_ShouldCheckAllDepartmentsOfWorkshop()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش تولید"));

        var result = await query.IsExistDepartmentName(workshop.Id, "بخش تولید", excludeDepartmentId: null);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenNameExistsInAnotherWorkshopOfSameUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش تولید"));

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه دوم")
            .WithNationalId("2222222222")
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop2);

        var result = await query.IsExistDepartmentName(workshop2.Id, "بخش تولید");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenNameExistsInAnotherUsersWorkshop_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();

        var user1Id = await CreateUserAsync(scope);
        var user2Id = await CreateUserAsync(scope, "09123456780");

        await CreateWorkshopWithDepartmentsAsync(scope, user1Id,
            (Guid.NewGuid(), "بخش تولید"));

        var workshop2 = await CreateWorkshopWithDepartmentsAsync(scope, user2Id,
            (Guid.NewGuid(), "بخش انبار"));

        var result = await query.IsExistDepartmentName(workshop2.Id, "بخش تولید");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistDepartmentName_WhenSameNameExistsInBothWorkshops_ShouldReturnTrueForMatchingWorkshop()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IDepartmentQuery>();
        var userId = await CreateUserAsync(scope);

        await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش تولید"));

        var workshop2 = await CreateWorkshopWithDepartmentsAsync(scope, userId,
            (Guid.NewGuid(), "بخش تولید"));

        var result = await query.IsExistDepartmentName(workshop2.Id, "بخش تولید");

        result.Should().BeTrue();
    }

    #endregion
}
