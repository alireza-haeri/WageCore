namespace Infrastructure.Tests.Repositories.EmployeeSalaryProfiles;

public class EmployeeSalaryProfileRepositoryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    private readonly WorkshopBuilder _workshopBuilder = new();
    private readonly EmployeeBuilder _employeeBuilder = new();
    private readonly EmployeeSalaryProfileBuilder _salaryProfileBuilder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> CreateUserAsync(AsyncServiceScope scope, string phoneNumber = "09123456789")
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        var user = new UserBuilder().WithPhoneNumber(phoneNumber).CreateResult().ShouldBeSuccess();
        var result = await userRepository.CreateAsync(user, "Pass123456");
        result.Succeeded.Should().BeTrue();
        return user.Id;
    }

    private async Task<Workshop> CreateWorkshopWithDepartmentAsync(AsyncServiceScope scope, Guid userId,
        Guid departmentId, string workshopName = "کارگاه نمونه", string nationalId = "1111111111")
    {
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName(workshopName)
            .WithRegistrationDate(ValidWorkshopRegistrationDate)
            .WithNationalId(nationalId)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);
        workshop.CreateDepartment(departmentId, "بخش تولید").ShouldBeSuccess();
        await repository.UpdateAsync(workshop);

        return workshop;
    }

    private async Task<Core.Domain.Employee> CreateEmployeeAsync(AsyncServiceScope scope, Guid workshopId,
        Guid departmentId, string personalCode = "EMP001", string fullName = "کارمند نمونه",
        string nationalCode = "1234567890")
    {
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();

        var employee = _employeeBuilder
            .WithId(Guid.NewGuid())
            .WithWorkshopId(workshopId)
            .WithDepartmentId(departmentId)
            .WithWorkshopRegistrationDate(ValidWorkshopRegistrationDate)
            .WithPersonalCode(personalCode)
            .WithFullName(fullName)
            .WithNationalCode(nationalCode)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(employee);
        result.Should().Be(employee.Id);

        return employee;
    }

    private EmployeeSalaryProfile CreateSalaryProfile(Core.Domain.Employee employee,
        DateOnly effectiveFrom, decimal baseMonthlySalary, decimal? housingAllowance = null)
    {
        return _salaryProfileBuilder
            .WithId(Guid.NewGuid())
            .WithEmployeeId(employee.Id)
            .WithEmployeeHireDate(employee.HireDate)
            .WithMinimumMonthlySalary(10_000_000m)
            .WithEffectiveFrom(effectiveFrom)
            .WithBaseMonthlySalary(baseMonthlySalary)
            .WithHousingAllowance(housingAllowance)
            .CreateResult()
            .ShouldBeSuccess();
    }

    [Fact]
    public async Task CreateAsync_WithValidSalaryProfile_ShouldPersistAndReturnItsId()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var effectiveFrom = employee.HireDate.AddDays(1);
        var salaryProfile = CreateSalaryProfile(employee, effectiveFrom, 20_000_000m, 1_400_000m);

        var result = await repository.CreateAsync(salaryProfile);

        result.Should().Be(salaryProfile.Id);

        var stored = await repository.GetByIdAsync(userId, salaryProfile.Id);
        stored.Should().NotBeNull();
        stored!.EmployeeId.Should().Be(employee.Id);
        stored.EffectiveFrom.Should().Be(effectiveFrom);
        stored.BaseMonthlySalary.Should().Be(20_000_000m);
        stored.HousingAllowance.Should().Be(1_400_000m);
    }

    [Fact]
    public async Task GetByIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var salaryProfile = CreateSalaryProfile(employee, employee.HireDate.AddDays(1), 20_000_000m);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var result = await repository.GetByIdAsync(anotherUserId, salaryProfile.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithWrongSalaryProfileId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var salaryProfile = CreateSalaryProfile(employee, employee.HireDate.AddDays(1), 20_000_000m);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var result = await repository.GetByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistSalaryProfileChanges()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var salaryProfile = CreateSalaryProfile(employee, employee.HireDate.AddDays(1), 20_000_000m);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var stored = await repository.GetByIdAsync(userId, salaryProfile.Id);
        stored.Should().NotBeNull();

        var newEffectiveFrom = employee.HireDate.AddDays(2);
        var newBaseMonthlySalary = 25_000_000m;
        var updateDto = new EmployeeSalaryProfileBuilder()
            .WithEffectiveFrom(newEffectiveFrom)
            .WithBaseMonthlySalary(newBaseMonthlySalary)
            .WithHousingAllowance(null)
            .BuildDto();

        stored!.Update(employee.HireDate, null, 10_000_000m, updateDto).ShouldBeSuccess();

        var updateResult = await repository.UpdateAsync(stored);

        updateResult.Should().BeTrue();

        var updated = await repository.GetByIdAsync(userId, salaryProfile.Id);
        updated.Should().NotBeNull();
        updated!.EffectiveFrom.Should().Be(newEffectiveFrom);
        updated.BaseMonthlySalary.Should().Be(newBaseMonthlySalary);
    }

    [Fact]
    public async Task DeleteAsync_WhenSalaryProfileExists_ShouldDeleteIt()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var salaryProfile = CreateSalaryProfile(employee, employee.HireDate.AddDays(1), 20_000_000m);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var result = await repository.DeleteAsync(userId, salaryProfile.Id);

        result.Should().BeTrue();
        (await repository.GetByIdAsync(userId, salaryProfile.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithWrongUserId_ShouldReturnFalseAndKeepSalaryProfile()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var salaryProfile = CreateSalaryProfile(employee, employee.HireDate.AddDays(1), 20_000_000m);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var result = await repository.DeleteAsync(anotherUserId, salaryProfile.Id);

        result.Should().BeFalse();
        (await repository.GetByIdAsync(userId, salaryProfile.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenSalaryProfileDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var userId = await CreateUserAsync(scope);

        var result = await repository.DeleteAsync(userId, Guid.NewGuid());

        result.Should().BeFalse();
    }
}
