namespace Infrastructure.Tests.Repositories.Workshops;

public class WorkshopRepositoryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private readonly WorkshopBuilder _workshopBuilder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> CreateUserAsync(AsyncServiceScope scope)
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        var user = new UserBuilder().CreateResult().ShouldBeSuccess();
        var result = await userRepository.CreateAsync(user,"Pass123456");
        result.Succeeded.Should().BeTrue();
        return user.Id;
    }

    [Fact]
    public async Task CreateAsync_WithValidWorkshop_ShouldSucceed()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(workshop);

        result.Should().NotBeNull();
        result.Value.Should().Be(workshop.Id);
    }

    [Fact]
    public async Task CreateAsync_WithOnlyRequiredFields_ShouldSucceed()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .WithPostalCode(null)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(workshop);

        result.Should().NotBeNull();
        result.Value.Should().Be(workshop.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenWorkshopExists_ShouldReturnWorkshop()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var result = await repository.GetByIdAsync(workshop.UserId, workshop.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(workshop.Id);
        result.UserId.Should().Be(workshop.UserId);
        result.Name.Should().Be(workshop.Name);
        result.Address.Should().Be(workshop.Address);
        result.RegistrationDate.Should().Be(workshop.RegistrationDate);
        result.NationalId.Should().Be(workshop.NationalId);
        result.PostalCode.Should().Be(workshop.PostalCode);
    }

    [Fact]
    public async Task GetByIdAsync_WhenWorkshopDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();

        var result = await repository.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), workshop.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldSucceed()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var updateResult = workshop.Update(
            "کارگاه جدید",
            "آدرس جدید، خیابان اصلی، پلاک ۲۰",
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            "9876543210",
            "0987654321");

        updateResult.ShouldBeSuccess();

        var result = await repository.UpdateAsync(workshop);

        result.Should().BeTrue();

        var updatedWorkshop = await repository.GetByIdAsync(workshop.UserId, workshop.Id);
        updatedWorkshop.Should().NotBeNull();
        updatedWorkshop!.Name.Should().Be("کارگاه جدید");
        updatedWorkshop.Address.Should().Be("آدرس جدید، خیابان اصلی، پلاک ۲۰");
        updatedWorkshop.RegistrationDate.Should().Be(DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));
        updatedWorkshop.NationalId.Should().Be("9876543210");
        updatedWorkshop.PostalCode.Should().Be("0987654321");
    }

    [Fact]
    public async Task UpdateAsync_WithNullPostalCode_ShouldSucceed()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .WithPostalCode("1234567890")
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var updateResult = workshop.Update(
            "کارگاه جدید",
            "آدرس جدید، خیابان اصلی، پلاک ۲۰",
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
            "9876543210",
            null);

        updateResult.ShouldBeSuccess();

        var result = await repository.UpdateAsync(workshop);

        result.Should().BeTrue();

        var updatedWorkshop = await repository.GetByIdAsync(workshop.UserId, workshop.Id);
        updatedWorkshop.Should().NotBeNull();
        updatedWorkshop!.PostalCode.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenWorkshopDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.UpdateAsync(workshop);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenWorkshopExists_ShouldSucceed()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var result = await repository.DeleteAsync(workshop.UserId, workshop.Id);

        result.Should().BeTrue();

        var deletedWorkshop = await repository.GetByIdAsync(workshop.UserId, workshop.Id);
        deletedWorkshop.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenWorkshopDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();

        var result = await repository.DeleteAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithWrongUserId_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var result = await repository.DeleteAsync(Guid.NewGuid(), workshop.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WithNewDepartment_ShouldPersistDepartment()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var department = workshop.CreateDepartment("بخش تولید").ShouldBeSuccess();

        var result = await repository.UpdateAsync(workshop);

        result.Should().BeTrue();

        var updatedWorkshop = await repository.GetByIdAsync(workshop.UserId, workshop.Id);
        updatedWorkshop.Should().NotBeNull();
        updatedWorkshop!.Departments.Should().ContainSingle();
        updatedWorkshop.Departments.First().Id.Should().Be(department.Id);
        updatedWorkshop.Departments.First().Name.Should().Be("بخش تولید");
        updatedWorkshop.Departments.First().WorkshopId.Should().Be(workshop.Id);
    }

    [Fact]
    public async Task UpdateAsync_WithUpdatedDepartment_ShouldPersistChanges()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var department = workshop.CreateDepartment("بخش تولید").ShouldBeSuccess();
        await repository.UpdateAsync(workshop);

        workshop.UpdateDepartment(department.Id, "بخش جدید").ShouldBeSuccess();
        var result = await repository.UpdateAsync(workshop);

        result.Should().BeTrue();

        var updatedWorkshop = await repository.GetByIdAsync(workshop.UserId, workshop.Id);
        updatedWorkshop.Should().NotBeNull();
        updatedWorkshop!.Departments.Should().ContainSingle();
        updatedWorkshop.Departments.First().Name.Should().Be("بخش جدید");
    }

    [Fact]
    public async Task UpdateAsync_WithDeletedDepartment_ShouldRemoveDepartment()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var department = workshop.CreateDepartment("بخش تولید").ShouldBeSuccess();
        await repository.UpdateAsync(workshop);

        workshop.DeleteDepartment(department.Id).ShouldBeSuccess();
        var result = await repository.UpdateAsync(workshop);

        result.Should().BeTrue();

        var updatedWorkshop = await repository.GetByIdAsync(workshop.UserId, workshop.Id);
        updatedWorkshop.Should().NotBeNull();
        updatedWorkshop!.Departments.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByDepartmentIdAsync_WhenDepartmentExists_ShouldReturnWorkshop()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var department = workshop.CreateDepartment("بخش تولید").ShouldBeSuccess();
        await repository.UpdateAsync(workshop);

        var result = await repository.GetByDepartmentIdAsync(userId, department.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(workshop.Id);
        result.Departments.Should().Contain(d => d.Id == department.Id);
    }

    [Fact]
    public async Task GetByDepartmentIdAsync_WhenDepartmentDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var result = await repository.GetByDepartmentIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDepartmentIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        var department = workshop.CreateDepartment("بخش تولید").ShouldBeSuccess();
        await repository.UpdateAsync(workshop);

        var result = await repository.GetByDepartmentIdAsync(Guid.NewGuid(), department.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeDepartments()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();
        await repository.CreateAsync(workshop);

        workshop.CreateDepartment("بخش اول").ShouldBeSuccess();
        workshop.CreateDepartment("بخش دوم").ShouldBeSuccess();
        await repository.UpdateAsync(workshop);

        var result = await repository.GetByIdAsync(userId, workshop.Id);

        result.Should().NotBeNull();
        result!.Departments.Should().HaveCount(2);
    }
}
