using FluentAssertions.Execution;

namespace Infrastructure.Tests.Repositories;

public class CascadeDeleteTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    private readonly WorkshopBuilder _workshopBuilder = new();
    private readonly EmployeeBuilder _employeeBuilder = new();

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
        string nationalId = "1111111111",
        params (Guid Id, string Name)[] departments)
    {
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithRegistrationDate(ValidWorkshopRegistrationDate)
            .WithNationalId(nationalId)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        foreach (var (id, name) in departments)
            workshop.CreateDepartment(id, name).ShouldBeSuccess();

        if (departments.Length > 0)
            await repository.UpdateAsync(workshop);

        return workshop;
    }

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

    private static async Task<int> CountDepartmentsAsync(AsyncServiceScope scope, Guid workshopId)
    {
        var context = scope.ServiceProvider.GetRequiredService<WageCoreDbContext>();
        return await context.Set<Department>().CountAsync(x => x.WorkshopId == workshopId);
    }

    private static async Task<int> CountEmployeesAsync(AsyncServiceScope scope, Guid workshopId)
    {
        var context = scope.ServiceProvider.GetRequiredService<WageCoreDbContext>();
        return await context.Employees.CountAsync(x => x.WorkshopId == workshopId);
    }

    private static async Task<int> CountBankAccountsAsync(AsyncServiceScope scope, Guid employeeId)
    {
        var context = scope.ServiceProvider.GetRequiredService<WageCoreDbContext>();
        return await context.Employees
            .Where(x => x.Id == employeeId)
            .SelectMany(x => x.BankAccounts)
            .CountAsync();
    }

    #region Delete Workshop

    [Fact]
    public async Task DeleteWorkshop_WhenWorkshopHasDepartmentsOnly_ShouldDeleteItsDepartments()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (Guid.NewGuid(), "بخش تولید"),
            (Guid.NewGuid(), "بخش انبار"));

        (await CountDepartmentsAsync(scope, workshop.Id)).Should().Be(2);

        var deleteResult = await workshopRepository.DeleteAsync(userId, workshop.Id);

        deleteResult.Should().BeTrue();
        (await CountDepartmentsAsync(scope, workshop.Id)).Should().Be(0);
    }

    /// <summary>
    /// Employees.WorkshopId is Restrict, but Employees.DepartmentId is Cascade and EF deletes the loaded
    /// Departments before the Workshop row. The department delete cascades the employees away in the database,
    /// so by the time the Workshop row is deleted nothing references it and the Restrict FK is never violated.
    /// This test pins down that end-to-end outcome so a future change to either FK breaks loudly.
    /// </summary>
    [Fact]
    public async Task DeleteWorkshop_WhenWorkshopHasEmployees_ShouldDeleteWorkshopDepartmentsAndEmployees()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var employeeRepository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (departmentId, "بخش تولید"));

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "1234567890");

        var deleteResult = await workshopRepository.DeleteAsync(userId, workshop.Id);

        deleteResult.Should().BeTrue();

        using (new AssertionScope())
        {
            (await workshopRepository.GetByIdAsync(userId, workshop.Id)).Should().BeNull();
            (await CountDepartmentsAsync(scope, workshop.Id)).Should().Be(0);
            (await CountEmployeesAsync(scope, workshop.Id)).Should().Be(0);
            (await employeeRepository.GetByIdAsync(userId, employee.Id)).Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteWorkshop_WhenWorkshopHasEmployees_ShouldDeleteTheirBankAccounts()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var employeeRepository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (departmentId, "بخش تولید"));

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "1234567890");

        employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("حساب اول", "IR111111111111111111111111"),
            new EmployeeBankAccountDto("حساب دوم", "IR222222222222222222222222")
        ]).ShouldBeSuccess();
        (await employeeRepository.UpdateAsync(employee)).Should().BeTrue();

        (await CountBankAccountsAsync(scope, employee.Id)).Should().Be(2);

        var deleteResult = await workshopRepository.DeleteAsync(userId, workshop.Id);

        deleteResult.Should().BeTrue();
        (await CountBankAccountsAsync(scope, employee.Id)).Should().Be(0);
    }

    [Fact]
    public async Task DeleteWorkshop_WhenEmployeesAreRemovedFirst_ShouldDeleteWorkshopAndDepartments()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var employeeRepository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (departmentId, "بخش تولید"));

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "1234567890");

        (await employeeRepository.DeleteAsync(userId, employee.Id)).Should().BeTrue();

        var deleteResult = await workshopRepository.DeleteAsync(userId, workshop.Id);

        deleteResult.Should().BeTrue();

        using (new AssertionScope())
        {
            (await workshopRepository.GetByIdAsync(userId, workshop.Id)).Should().BeNull();
            (await CountDepartmentsAsync(scope, workshop.Id)).Should().Be(0);
            (await CountEmployeesAsync(scope, workshop.Id)).Should().Be(0);
        }
    }

    [Fact]
    public async Task DeleteWorkshop_ShouldNotAffectDepartmentsOfOtherWorkshops()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (Guid.NewGuid(), "بخش تولید"));

        var otherDepartmentId = Guid.NewGuid();
        var workshop2 = await CreateWorkshopWithDepartmentsAsync(scope, userId, "2222222222",
            (otherDepartmentId, "بخش اداری"));

        await CreateEmployeeAsync(scope, workshop2.Id, otherDepartmentId, "EMP002", "0987654321");

        var deleteResult = await workshopRepository.DeleteAsync(userId, workshop1.Id);

        deleteResult.Should().BeTrue();

        using (new AssertionScope())
        {
            (await CountDepartmentsAsync(scope, workshop1.Id)).Should().Be(0);
            (await CountDepartmentsAsync(scope, workshop2.Id)).Should().Be(1);
            (await CountEmployeesAsync(scope, workshop2.Id)).Should().Be(1);
        }
    }

    #endregion

    #region Delete Department

    [Fact]
    public async Task DeleteDepartment_WhenDepartmentHasEmployees_ShouldDeleteThoseEmployees()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var employeeRepository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (departmentId, "بخش تولید"));

        var employee1 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "1234567890");
        var employee2 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "0987654321");

        var storedWorkshop = await workshopRepository.GetByIdAsync(userId, workshop.Id);
        storedWorkshop.Should().NotBeNull();
        storedWorkshop!.DeleteDepartment(departmentId).ShouldBeSuccess();

        var updateResult = await workshopRepository.UpdateAsync(storedWorkshop);

        updateResult.Should().BeTrue();

        using (new AssertionScope())
        {
            (await CountDepartmentsAsync(scope, workshop.Id)).Should().Be(0);
            (await employeeRepository.GetByIdAsync(userId, employee1.Id)).Should().BeNull();
            (await employeeRepository.GetByIdAsync(userId, employee2.Id)).Should().BeNull();
            (await CountEmployeesAsync(scope, workshop.Id)).Should().Be(0);
        }
    }

    [Fact]
    public async Task DeleteDepartment_ShouldDeleteBankAccountsOfItsEmployees()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var employeeRepository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (departmentId, "بخش تولید"));

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "1234567890");

        employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("حساب اول", "IR111111111111111111111111"),
            new EmployeeBankAccountDto("حساب دوم", "IR222222222222222222222222")
        ]).ShouldBeSuccess();
        (await employeeRepository.UpdateAsync(employee)).Should().BeTrue();

        (await CountBankAccountsAsync(scope, employee.Id)).Should().Be(2);

        var storedWorkshop = await workshopRepository.GetByIdAsync(userId, workshop.Id);
        storedWorkshop!.DeleteDepartment(departmentId).ShouldBeSuccess();
        (await workshopRepository.UpdateAsync(storedWorkshop)).Should().BeTrue();

        using (new AssertionScope())
        {
            (await employeeRepository.GetByIdAsync(userId, employee.Id)).Should().BeNull();
            (await CountBankAccountsAsync(scope, employee.Id)).Should().Be(0);
        }
    }

    [Fact]
    public async Task DeleteDepartment_ShouldNotAffectEmployeesOfOtherDepartments()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var employeeRepository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);

        var departmentId1 = Guid.NewGuid();
        var departmentId2 = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (departmentId1, "بخش تولید"),
            (departmentId2, "بخش انبار"));

        var removedEmployee = await CreateEmployeeAsync(scope, workshop.Id, departmentId1, "EMP001", "1234567890");
        var keptEmployee = await CreateEmployeeAsync(scope, workshop.Id, departmentId2, "EMP002", "0987654321");

        var storedWorkshop = await workshopRepository.GetByIdAsync(userId, workshop.Id);
        storedWorkshop!.DeleteDepartment(departmentId1).ShouldBeSuccess();

        var updateResult = await workshopRepository.UpdateAsync(storedWorkshop);

        updateResult.Should().BeTrue();

        using (new AssertionScope())
        {
            (await CountDepartmentsAsync(scope, workshop.Id)).Should().Be(1);
            (await employeeRepository.GetByIdAsync(userId, removedEmployee.Id)).Should().BeNull();

            var survivor = await employeeRepository.GetByIdAsync(userId, keptEmployee.Id);
            survivor.Should().NotBeNull();
            survivor!.DepartmentId.Should().Be(departmentId2);
            (await CountEmployeesAsync(scope, workshop.Id)).Should().Be(1);
        }
    }

    [Fact]
    public async Task DeleteDepartment_WhenDepartmentHasNoEmployees_ShouldOnlyDeleteDepartment()
    {
        await using var scope = fixture.CreateScope();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);

        var emptyDepartmentId = Guid.NewGuid();
        var usedDepartmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentsAsync(scope, userId, "1111111111",
            (emptyDepartmentId, "بخش خالی"),
            (usedDepartmentId, "بخش تولید"));

        await CreateEmployeeAsync(scope, workshop.Id, usedDepartmentId, "EMP001", "1234567890");

        var storedWorkshop = await workshopRepository.GetByIdAsync(userId, workshop.Id);
        storedWorkshop!.DeleteDepartment(emptyDepartmentId).ShouldBeSuccess();

        var updateResult = await workshopRepository.UpdateAsync(storedWorkshop);

        updateResult.Should().BeTrue();

        using (new AssertionScope())
        {
            (await CountDepartmentsAsync(scope, workshop.Id)).Should().Be(1);
            (await CountEmployeesAsync(scope, workshop.Id)).Should().Be(1);
        }
    }

    #endregion
}
