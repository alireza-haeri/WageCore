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

    #region GetAnnualWorkedDaysCountAsync

    // The payroll periods used here (April-August 2025 and February 2025) sit
    // well inside Persian years 1404 and 1403 respectively, whichever day .NET
    // picks for Nowruz, so the year classification is stable.

    [Fact]
    public async Task GetAnnualWorkedDaysCountAsync_WhenClosedRecordsExistEarlierInTheSamePersianYear_ShouldSumTheirWorkedDays()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        // Two closed periods of 1404 before the current period: 24 worked days each.
        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 4, 20), new DateOnly(2025, 5, 14));
        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 5, 15), new DateOnly(2025, 6, 14));

        var result = await query.GetAnnualWorkedDaysCountAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 7, 1));

        result.Should().Be(48m);
    }

    [Fact]
    public async Task GetAnnualWorkedDaysCountAsync_WhenTheCurrentPeriodIsAlreadyPersisted_ShouldExcludeIt()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 4, 20), new DateOnly(2025, 5, 14));
        // The payroll record of the very period being calculated is persisted
        // during an update flow, so it must not be counted.
        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 7, 1), new DateOnly(2025, 7, 31));

        var result = await query.GetAnnualWorkedDaysCountAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 7, 1));

        result.Should().Be(24m);
    }

    [Fact]
    public async Task GetAnnualWorkedDaysCountAsync_ShouldExcludeRecordsOfThePreviousPersianYear()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        // February 2025 belongs to Persian year 1403; the current period (July) is 1404.
        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 2, 1), new DateOnly(2025, 2, 28));
        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 4, 20), new DateOnly(2025, 5, 14));

        var result = await query.GetAnnualWorkedDaysCountAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 7, 1));

        result.Should().Be(24m);
    }

    [Fact]
    public async Task GetAnnualWorkedDaysCountAsync_ShouldExcludeRecordsOfLaterMonthsInTheSameYear()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 4, 20), new DateOnly(2025, 5, 14));
        // A later month of the same Persian year has not ended before the current
        // period started, so it is not part of the year-to-date aggregation.
        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 8, 1), new DateOnly(2025, 8, 31));

        var result = await query.GetAnnualWorkedDaysCountAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 7, 1));

        result.Should().Be(24m);
    }

    [Fact]
    public async Task GetAnnualWorkedDaysCountAsync_WithAnotherUser_ShouldReturnZero()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        await SavePayrollRecordAsync(scope, employee, new DateOnly(2025, 4, 20), new DateOnly(2025, 5, 14));

        var result = await query.GetAnnualWorkedDaysCountAsync(
            anotherUserId,
            employee.Id,
            new DateOnly(2025, 7, 1));

        result.Should().Be(0m);
    }

    [Fact]
    public async Task GetAnnualWorkedDaysCountAsync_WhenTheEmployeeHasNoRecords_ShouldReturnZero()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IPayrollRecordQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId);
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId);

        var result = await query.GetAnnualWorkedDaysCountAsync(
            userId,
            employee.Id,
            new DateOnly(2025, 7, 1));

        result.Should().Be(0m);
    }

    #endregion
}
