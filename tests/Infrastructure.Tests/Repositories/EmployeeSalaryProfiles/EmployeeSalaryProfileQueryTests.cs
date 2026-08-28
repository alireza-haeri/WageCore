namespace Infrastructure.Tests.Repositories.EmployeeSalaryProfiles;

public class EmployeeSalaryProfileQueryTests(WageCoreDbContextFixture fixture)
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
        Guid departmentId, string workshopName, string departmentName, string nationalId)
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
        workshop.CreateDepartment(departmentId, departmentName).ShouldBeSuccess();
        await repository.UpdateAsync(workshop);

        return workshop;
    }

    private async Task<Core.Domain.Employee> CreateEmployeeAsync(AsyncServiceScope scope, Guid workshopId,
        Guid departmentId, string personalCode, string fullName, string nationalCode)
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
        DateOnly effectiveFrom, decimal baseMonthlySalary)
    {
        return _salaryProfileBuilder
            .WithId(Guid.NewGuid())
            .WithEmployeeId(employee.Id)
            .WithEmployeeHireDate(employee.HireDate)
            .WithMinimumMonthlySalary(10_000_000m)
            .WithEffectiveFrom(effectiveFrom)
            .WithBaseMonthlySalary(baseMonthlySalary)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private async Task<EmployeeSalaryProfile> SaveSalaryProfileAsync(AsyncServiceScope scope,
        Core.Domain.Employee employee, DateOnly effectiveFrom, decimal baseMonthlySalary)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var salaryProfile = CreateSalaryProfile(employee, effectiveFrom, baseMonthlySalary);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);
        return salaryProfile;
    }

    #region GetLatestEffectiveFromAsync

    [Fact]
    public async Task GetLatestEffectiveFromAsync_WhenNoProfileExists_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var result = await query.GetLatestEffectiveFromAsync(userId, employee.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestEffectiveFromAsync_ShouldReturnLatestEffectiveFrom()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var olderDate = employee.HireDate.AddDays(1);
        var newerDate = employee.HireDate.AddDays(2);
        await SaveSalaryProfileAsync(scope, employee, olderDate, 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee, newerDate, 25_000_000m);

        var result = await query.GetLatestEffectiveFromAsync(userId, employee.Id);

        result.Should().Be(newerDate);
    }

    [Fact]
    public async Task GetLatestEffectiveFromAsync_WithExcludeId_ShouldIgnoreThatProfile()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var olderDate = employee.HireDate.AddDays(1);
        var newerDate = employee.HireDate.AddDays(2);
        await SaveSalaryProfileAsync(scope, employee, olderDate, 20_000_000m);
        var newest = await SaveSalaryProfileAsync(scope, employee, newerDate, 25_000_000m);

        var result = await query.GetLatestEffectiveFromAsync(userId, employee.Id, newest.Id);

        result.Should().Be(olderDate);
    }

    [Fact]
    public async Task GetLatestEffectiveFromAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(1), 20_000_000m);

        var result = await query.GetLatestEffectiveFromAsync(anotherUserId, employee.Id);

        result.Should().BeNull();
    }

    #endregion

    #region GetEmployeeSalaryProfilesAsync

    [Fact]
    public async Task GetEmployeeSalaryProfilesAsync_WithoutFilters_ShouldReturnAllProfilesOrderedByEffectiveFrom()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var olderDate = employee.HireDate.AddDays(1);
        var newerDate = employee.HireDate.AddDays(2);
        await SaveSalaryProfileAsync(scope, employee, olderDate, 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee, newerDate, 25_000_000m);

        var result = await query.GetEmployeeSalaryProfilesAsync(userId, new PaginationDto(1, 10));

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].EffectiveFrom.Should().Be(newerDate);
        result.Items[0].BaseMonthlySalary.Should().Be(25_000_000m);
        result.Items[1].EffectiveFrom.Should().Be(olderDate);
        result.Items[1].BaseMonthlySalary.Should().Be(20_000_000m);
        result.Items.Should().OnlyContain(x => x.EmployeeName == "علی رضایی");
        result.Items.Should().OnlyContain(x => x.PersonalCode == "EMP001");
        result.Items.Should().OnlyContain(x => x.WorkshopName == "کارگاه نمونه");
        result.Items.Should().OnlyContain(x => x.DepartmentName == "بخش تولید");
    }

    [Fact]
    public async Task GetEmployeeSalaryProfilesAsync_WithEmployeeIdFilter_ShouldReturnOnlyThatEmployee()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee1 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");
        var employee2 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "مینا احمدی", "0987654321");

        await SaveSalaryProfileAsync(scope, employee1, employee1.HireDate.AddDays(1), 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee2, employee2.HireDate.AddDays(1), 21_000_000m);

        var result = await query.GetEmployeeSalaryProfilesAsync(
            userId, new PaginationDto(1, 10), employeeId: employee1.Id);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().EmployeeId.Should().Be(employee1.Id);
    }

    [Fact]
    public async Task GetEmployeeSalaryProfilesAsync_WithSearch_ShouldFilterByFullName()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee1 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");
        var employee2 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "مینا احمدی", "0987654321");

        await SaveSalaryProfileAsync(scope, employee1, employee1.HireDate.AddDays(1), 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee2, employee2.HireDate.AddDays(1), 21_000_000m);

        var result = await query.GetEmployeeSalaryProfilesAsync(
            userId, new PaginationDto(1, 10), search: "رضا");

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().EmployeeName.Should().Be("علی رضایی");
    }

    [Fact]
    public async Task GetEmployeeSalaryProfilesAsync_WithStatusFilter_ShouldReturnOnlyRequestedStatus()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var olderDate = employee.HireDate.AddDays(1);
        var newerDate = employee.HireDate.AddDays(2);
        await SaveSalaryProfileAsync(scope, employee, olderDate, 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee, newerDate, 25_000_000m);

        var activeResult = await query.GetEmployeeSalaryProfilesAsync(
            userId, new PaginationDto(1, 10), status: EmployeeSalaryProfileStatus.Active);
        var expiredResult = await query.GetEmployeeSalaryProfilesAsync(
            userId, new PaginationDto(1, 10), status: EmployeeSalaryProfileStatus.Expired);

        activeResult.TotalCount.Should().Be(1);
        activeResult.Items.Should().ContainSingle();
        activeResult.Items.First().EffectiveFrom.Should().Be(newerDate);
        activeResult.Items.First().Status.Should().Be(EmployeeSalaryProfileStatus.Active);

        expiredResult.TotalCount.Should().Be(1);
        expiredResult.Items.Should().ContainSingle();
        expiredResult.Items.First().EffectiveFrom.Should().Be(olderDate);
        expiredResult.Items.First().Status.Should().Be(EmployeeSalaryProfileStatus.Expired);
    }

    [Fact]
    public async Task GetEmployeeSalaryProfilesAsync_WithWorkshopAndDepartmentFilter_ShouldFilterResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId1 = Guid.NewGuid();
        var workshop1 = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId1,
            "کارگاه اول", "بخش تولید", "1111111111");
        var departmentId2 = Guid.NewGuid();
        var workshop2 = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId2,
            "کارگاه دوم", "بخش اداری", "2222222222");

        var employee1 = await CreateEmployeeAsync(scope, workshop1.Id, departmentId1, "EMP001", "علی رضایی", "1234567890");
        var employee2 = await CreateEmployeeAsync(scope, workshop2.Id, departmentId2, "EMP002", "مینا احمدی", "0987654321");

        await SaveSalaryProfileAsync(scope, employee1, employee1.HireDate.AddDays(1), 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee2, employee2.HireDate.AddDays(1), 21_000_000m);

        var result = await query.GetEmployeeSalaryProfilesAsync(
            userId, new PaginationDto(1, 10), workshopId: workshop2.Id, departmentId: departmentId2);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().WorkshopName.Should().Be("کارگاه دوم");
        result.Items.First().DepartmentName.Should().Be("بخش اداری");
    }

    [Fact]
    public async Task GetEmployeeSalaryProfilesAsync_WithPagination_ShouldPaginateCorrectly()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(1), 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(2), 21_000_000m);
        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(3), 22_000_000m);

        var result = await query.GetEmployeeSalaryProfilesAsync(userId, new PaginationDto(1, 2));

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetEmployeeSalaryProfilesAsync_WithWrongUserId_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(1), 20_000_000m);

        var result = await query.GetEmployeeSalaryProfilesAsync(anotherUserId, new PaginationDto(1, 10));

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    #endregion

    #region GetEmployeeSalaryProfileByIdAsync

    [Fact]
    public async Task GetEmployeeSalaryProfileByIdAsync_WhenProfileExists_ShouldReturnFullProfile()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var effectiveFrom = employee.HireDate.AddDays(1);
        var salaryProfile = _salaryProfileBuilder
            .WithId(Guid.NewGuid())
            .WithEmployeeId(employee.Id)
            .WithEmployeeHireDate(employee.HireDate)
            .WithMinimumMonthlySalary(10_000_000m)
            .WithEffectiveFrom(effectiveFrom)
            .WithBaseMonthlySalary(20_000_000m)
            .WithAttractionAllowance(1_000_000m)
            .WithSupervisionAllowance(2_000_000m)
            .WithSeniorityBaseApplicationMode(SeniorityBaseApplicationMode.Automatic)
            .WithSeniorityBaseCalculationMethod(SeniorityBaseCalculationMethod.CumulativeAuto)
            .WithYearEndSeniorityMode(YearEndSeniorityMode.AnnualLumpSum)
            .WithShiftType(ShiftType.MorningEvening)
            .WithHousingAllowance(1_400_000m)
            .WithFoodAllowance(2_200_000m)
            .WithChildAllowancePerChild(1_044_686m)
            .WithTransportationAllowanceNet(500_000m)
            .WithKaranehAmountNet(300_000m)
            .CreateResult()
            .ShouldBeSuccess();

        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var result = await query.GetEmployeeSalaryProfileByIdAsync(userId, salaryProfile.Id);

        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(employee.Id);
        result.EffectiveFrom.Should().Be(effectiveFrom);
        result.BaseMonthlySalary.Should().Be(20_000_000m);
        result.AttractionAllowance.Should().Be(1_000_000m);
        result.SupervisionAllowance.Should().Be(2_000_000m);
        result.SeniorityBaseApplicationMode.Should().Be(SeniorityBaseApplicationMode.Automatic);
        result.SeniorityBaseCalculationMethod.Should().Be(SeniorityBaseCalculationMethod.CumulativeAuto);
        result.YearEndSeniorityMode.Should().Be(YearEndSeniorityMode.AnnualLumpSum);
        result.ShiftType.Should().Be(ShiftType.MorningEvening);
        result.HousingAllowance.Should().Be(1_400_000m);
        result.FoodAllowance.Should().Be(2_200_000m);
        result.ChildAllowancePerChild.Should().Be(1_044_686m);
        result.TransportationAllowanceNet.Should().Be(500_000m);
        result.KaranehAmountNet.Should().Be(300_000m);
    }

    [Fact]
    public async Task GetEmployeeSalaryProfileByIdAsync_WithNullOptionalFields_ShouldReturnNulls()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var salaryProfile = CreateSalaryProfile(employee, employee.HireDate.AddDays(1), 20_000_000m);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var result = await query.GetEmployeeSalaryProfileByIdAsync(userId, salaryProfile.Id);

        result.Should().NotBeNull();
        result!.AttractionAllowance.Should().BeNull();
        result.SupervisionAllowance.Should().BeNull();
        result.SeniorityBaseApplicationMode.Should().Be(SeniorityBaseApplicationMode.Manual);
        result.SeniorityBaseCalculationMethod.Should().BeNull();
        result.HousingAllowance.Should().BeNull();
        result.FoodAllowance.Should().BeNull();
        result.ChildAllowancePerChild.Should().BeNull();
        result.TransportationAllowanceNet.Should().BeNull();
        result.KaranehAmountNet.Should().BeNull();
    }

    [Fact]
    public async Task GetEmployeeSalaryProfileByIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var salaryProfile = await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(1), 20_000_000m);

        var result = await query.GetEmployeeSalaryProfileByIdAsync(anotherUserId, salaryProfile.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEmployeeSalaryProfileByIdAsync_WithWrongSalaryProfileId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeSalaryProfileQuery>();
        var userId = await CreateUserAsync(scope);

        var result = await query.GetEmployeeSalaryProfileByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion
}
