namespace Infrastructure.Tests.Repositories.PayrollRecords;

public class PayrollRecordQueryTests(WageCoreDbContextFixture fixture)
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

    private async Task<PayrollRecord> SavePayrollRecordAsync(AsyncServiceScope scope,
        Core.Domain.Employee employee, DateOnly periodStart, DateOnly periodEnd)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IPayrollRecordRepository>();

        var payrollRecord = _payrollRecordBuilder
            .WithId(Guid.NewGuid())
            .WithEmployeeId(employee.Id)
            .WithPeriod(periodStart, periodEnd)
            .CreateResult()
            .ShouldBeSuccess();

        (await repository.CreateAsync(payrollRecord)).Should().Be(payrollRecord.Id);
        return payrollRecord;
    }

    #region HasOverlappingPeriodAsync

    [Fact]
    public async Task HasOverlappingPeriodAsync_WhenAnotherRecordOverlaps_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        var result = await query.HasOverlappingPeriodAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 1, 15),
            new DateOnly(2025, 2, 14));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasOverlappingPeriodAsync_WhenNoRecordOverlaps_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        var result = await query.HasOverlappingPeriodAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 2, 1),
            new DateOnly(2025, 2, 28));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasOverlappingPeriodAsync_WithExcludeId_ShouldIgnoreThatRecord()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var payrollRecord = await SavePayrollRecordAsync(
            scope, employee, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        var result = await query.HasOverlappingPeriodAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 1, 31),
            payrollRecord.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasOverlappingPeriodAsync_WithAnotherUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        var result = await query.HasOverlappingPeriodAsync(
            anotherUserId,
            employee.Id,
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 1, 31));

        result.Should().BeFalse();
    }

    #endregion

    #region HasPayrollRecordEffectAsync

    [Fact]
    public async Task HasPayrollRecordEffectAsync_WhenARecordExtendsToTheEffectiveFrom_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        var result = await query.HasPayrollRecordEffectAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 1, 15));

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPayrollRecordEffectAsync_WhenNoRecordReachesTheEffectiveFrom_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        var result = await query.HasPayrollRecordEffectAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 2, 1));

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPayrollRecordEffectAsync_WithAnotherUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31));

        var result = await query.HasPayrollRecordEffectAsync(
            anotherUserId,
            employee.Id,
            new DateOnly(2025, 1, 1));

        result.Should().BeFalse();
    }

    #endregion
}
