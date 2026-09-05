namespace Infrastructure.Tests.Repositories.PayrollRecords;

public class PayrollRecordRepositoryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    private readonly WorkshopBuilder _workshopBuilder = new();
    private readonly EmployeeBuilder _employeeBuilder = new();
    private readonly PayrollRecordBuilder _payrollRecordBuilder = new();

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

    private PayrollRecord CreatePayrollRecord(Core.Domain.Employee employee,
        DateOnly periodStart, DateOnly periodEnd)
    {
        return _payrollRecordBuilder
            .WithId(Guid.NewGuid())
            .WithEmployeeId(employee.Id)
            .WithPeriod(periodStart, periodEnd)
            .CreateResult()
            .ShouldBeSuccess();
    }

    [Fact]
    public async Task CreateAsync_WithValidPayrollRecord_ShouldPersistAndReturnItsId()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPayrollRecordRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var periodStart = new DateOnly(2025, 2, 1);
        var periodEnd = new DateOnly(2025, 2, 28);
        var payrollRecord = CreatePayrollRecord(employee, periodStart, periodEnd);

        var result = await repository.CreateAsync(payrollRecord);

        result.Should().Be(payrollRecord.Id);

        var stored = await repository.GetByIdAsync(userId, payrollRecord.Id);
        stored.Should().NotBeNull();
        stored!.EmployeeId.Should().Be(employee.Id);
        stored.PeriodStart.Should().Be(periodStart);
        stored.PeriodEnd.Should().Be(periodEnd);
        stored.Status.Should().Be(PayrollRecordStatus.Draft);
        stored.GrossAmount.Should().Be(17_900_000m);
        stored.InsuranceAmount.Should().Be(1_400_000m);
        stored.CalculatedTaxAmount.Should().Be(1_500_000m);
        stored.TotalDeductionsAmount.Should().Be(2_900_000m);
        stored.NetPayableAmount.Should().Be(15_000_000m);
    }

    [Fact]
    public async Task GetByIdAsync_WithAnotherUser_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPayrollRecordRepository>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var payrollRecord = CreatePayrollRecord(employee, new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28));
        (await repository.CreateAsync(payrollRecord)).Should().Be(payrollRecord.Id);

        var stored = await repository.GetByIdAsync(anotherUserId, payrollRecord.Id);

        stored.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistTheChanges()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPayrollRecordRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var payrollRecord = CreatePayrollRecord(employee, new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28));
        (await repository.CreateAsync(payrollRecord)).Should().Be(payrollRecord.Id);

        var stored = await repository.GetByIdAsync(userId, payrollRecord.Id);
        stored.Should().NotBeNull();
        stored!.MarkAsPaid().ShouldBeSuccess();

        var updateResult = await repository.UpdateAsync(stored);

        updateResult.Should().BeTrue();

        var reloaded = await repository.GetByIdAsync(userId, payrollRecord.Id);
        reloaded.Should().NotBeNull();
        reloaded!.Status.Should().Be(PayrollRecordStatus.Paid);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveThePayrollRecord()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPayrollRecordRepository>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var payrollRecord = CreatePayrollRecord(employee, new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28));
        (await repository.CreateAsync(payrollRecord)).Should().Be(payrollRecord.Id);

        var deleteResult = await repository.DeleteAsync(userId, payrollRecord.Id);

        deleteResult.Should().BeTrue();
        (await repository.GetByIdAsync(userId, payrollRecord.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithAnotherUser_ShouldReturnFalseAndKeepThePayrollRecord()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPayrollRecordRepository>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var payrollRecord = CreatePayrollRecord(employee, new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28));
        (await repository.CreateAsync(payrollRecord)).Should().Be(payrollRecord.Id);

        var deleteResult = await repository.DeleteAsync(anotherUserId, payrollRecord.Id);

        deleteResult.Should().BeFalse();
        (await repository.GetByIdAsync(userId, payrollRecord.Id)).Should().NotBeNull();
    }
}
