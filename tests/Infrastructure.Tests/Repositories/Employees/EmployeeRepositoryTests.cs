namespace Infrastructure.Tests.Repositories.Employees;

public class EmployeeRepositoryTests(WageCoreDbContextFixture fixture)
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

    private async Task<Workshop> CreateWorkshopWithDepartmentAsync(AsyncServiceScope scope, Guid userId, Guid departmentId,
        string departmentName = "بخش تولید")
    {
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithRegistrationDate(ValidWorkshopRegistrationDate)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);
        workshop.CreateDepartment(departmentId, departmentName).ShouldBeSuccess();
        await repository.UpdateAsync(workshop);

        return workshop;
    }

    private async Task<Core.Domain.Employee> CreateEmployeeAsync(AsyncServiceScope scope, Guid workshopId, Guid departmentId,
        string personalCode = "EMP001", string fullName = "کارمند نمونه", string nationalCode = "1234567890")
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

    [Fact]
    public async Task CreateAsync_WithValidEmployee_ShouldPersistEmployee()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var employee = _employeeBuilder
            .WithId(Guid.NewGuid())
            .WithWorkshopId(workshop.Id)
            .WithDepartmentId(departmentId)
            .WithPersonalCode("EMP100")
            .WithFullName("علی رضایی")
            .WithNationalCode("1234512345")
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(employee);

        result.Should().Be(employee.Id);

        var storedEmployee = await repository.GetByIdAsync(userId, employee.Id);
        storedEmployee.Should().NotBeNull();
        storedEmployee!.PersonalCode.Should().Be("EMP100");
        storedEmployee.FullName.Should().Be("علی رضایی");
        storedEmployee.NationalCode.Should().Be("1234512345");
        storedEmployee.WorkshopId.Should().Be(workshop.Id);
        storedEmployee.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEmployeeExists_ShouldReturnEmployeeWithBankAccounts()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "IR123456789012345678901234")
        ]).ShouldBeSuccess();
        await repository.UpdateAsync(employee);

        var result = await repository.GetByIdAsync(userId, employee.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(employee.Id);
        result.BankAccounts.Should().ContainSingle();
        result.BankAccounts.First().BankName.Should().Be("بانک ملی");
        result.BankAccounts.First().BranchCode.Should().Be("۱۰۲");
        result.BankAccounts.First().Iban.Should().Be("123456789012345678901234");
    }

    [Fact]
    public async Task GetByIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var result = await repository.GetByIdAsync(Guid.NewGuid(), employee.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeExists_ShouldPersistChanges()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var updateDto = new EmployeeDto(
            departmentId,
            "EMP999",
            "رضا محمدی",
            "1111122222",
            employee.FatherName,
            employee.Gender,
            employee.HireDate,
            employee.PhoneNumber,
            "مدیر مالی",
            employee.Region);

        employee.Update(updateDto, workshop.RegistrationDate).ShouldBeSuccess();

        var updateResult = await repository.UpdateAsync(employee);

        updateResult.Should().BeTrue();

        var storedEmployee = await repository.GetByIdAsync(userId, employee.Id);
        storedEmployee.Should().NotBeNull();
        storedEmployee!.PersonalCode.Should().Be("EMP999");
        storedEmployee.FullName.Should().Be("رضا محمدی");
        storedEmployee.NationalCode.Should().Be("1111122222");
        storedEmployee.JobTitle.Should().Be("مدیر مالی");
        storedEmployee.Region.Should().Be(employee.Region);
    }

    [Fact]
    public async Task DeleteAsync_WhenEmployeeExists_ShouldDeleteEmployee()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var deleteResult = await repository.DeleteAsync(userId, employee.Id);

        deleteResult.Should().BeTrue();

        var storedEmployee = await repository.GetByIdAsync(userId, employee.Id);
        storedEmployee.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithWrongUserId_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var deleteResult = await repository.DeleteAsync(Guid.NewGuid(), employee.Id);

        deleteResult.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeIsTerminated_ShouldPersistTerminationDate()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);
        var terminationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        employee.Terminate(terminationDate).ShouldBeSuccess();

        var updateResult = await repository.UpdateAsync(employee);

        updateResult.Should().BeTrue();

        var storedEmployee = await repository.GetByIdAsync(userId, employee.Id);
        storedEmployee.Should().NotBeNull();
        storedEmployee!.TerminationDate.Should().Be(terminationDate);
        storedEmployee.IsTerminated.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeIsRehired_ShouldClearTerminationDateAndPersistNewHireData()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);
        var terminationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-3));
        var rehireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        employee.Terminate(terminationDate).ShouldBeSuccess();
        await repository.UpdateAsync(employee);

        employee.Rehire(new EmployeeRehireDto(
            departmentId,
            workshop.RegistrationDate,
            rehireDate)).ShouldBeSuccess();

        var updateResult = await repository.UpdateAsync(employee);

        updateResult.Should().BeTrue();

        var storedEmployee = await repository.GetByIdAsync(userId, employee.Id);
        storedEmployee.Should().NotBeNull();
        storedEmployee!.TerminationDate.Should().BeNull();
        storedEmployee.IsTerminated.Should().BeFalse();
        storedEmployee.HireDate.Should().Be(rehireDate);
        storedEmployee.DepartmentId.Should().Be(departmentId);
    }

    [Fact]
    public async Task UpdateAsync_WhenEmployeeIsRehiredInAnotherDepartment_ShouldPersistNewDepartment()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var workshopRepository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);

        var newDepartmentId = Guid.NewGuid();
        workshop.CreateDepartment(newDepartmentId, "بخش اداری").ShouldBeSuccess();
        await workshopRepository.UpdateAsync(workshop);

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        employee.Terminate(DateOnly.FromDateTime(DateTime.Now.AddDays(-3))).ShouldBeSuccess();
        await repository.UpdateAsync(employee);

        employee.Rehire(new EmployeeRehireDto(
            newDepartmentId,
            workshop.RegistrationDate,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-1)))).ShouldBeSuccess();

        var updateResult = await repository.UpdateAsync(employee);

        updateResult.Should().BeTrue();

        var storedEmployee = await repository.GetByIdAsync(userId, employee.Id);
        storedEmployee.Should().NotBeNull();
        storedEmployee!.DepartmentId.Should().Be(newDepartmentId);
        storedEmployee.TerminationDate.Should().BeNull();
    }
}
