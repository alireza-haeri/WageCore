namespace Infrastructure.Tests.Repositories.SalaryDecrees;

public class SalaryDecreeQueryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private static readonly DateOnly ValidWorkshopRegistrationDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-30));

    private readonly WorkshopBuilder _workshopBuilder = new();
    private readonly EmployeeBuilder _employeeBuilder = new();
    private readonly SalaryDecreeBuilder _salaryProfileBuilder = new();

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

    private SalaryDecree CreateSalaryProfile(Core.Domain.Employee employee,
        DateOnly effectiveFrom, decimal baseDailySalary)
    {
        return _salaryProfileBuilder
            .WithId(Guid.NewGuid())
            .WithEmployeeId(employee.Id)
            .WithEmployeeHireDate(employee.HireDate)
            .WithMinimumMonthlySalary(10_000_000m)
            .WithEffectiveFrom(effectiveFrom)
            .WithBaseDailySalary(baseDailySalary)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private async Task<SalaryDecree> SaveSalaryProfileAsync(AsyncServiceScope scope,
        Core.Domain.Employee employee, DateOnly effectiveFrom, decimal baseDailySalary)
    {
        var repository = scope.ServiceProvider.GetRequiredService<ISalaryDecreeRepository>();
        var salaryProfile = CreateSalaryProfile(employee, effectiveFrom, baseDailySalary);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);
        return salaryProfile;
    }

    #region GetLatestEffectiveFromAsync

    [Fact]
    public async Task GetLatestEffectiveFromAsync_WhenNoProfileExists_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
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
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
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
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
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
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
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

    #region GetSalaryDecreesAsync

    [Fact]
    public async Task GetSalaryDecreesAsync_WithoutFilters_ShouldReturnAllProfilesOrderedByEffectiveFrom()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var olderDate = employee.HireDate.AddDays(1);
        var newerDate = employee.HireDate.AddDays(2);
        await SaveSalaryProfileAsync(scope, employee, olderDate, 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee, newerDate, 25_000_000m);

        var result = await query.GetSalaryDecreesAsync(userId, new PaginationDto(1, 10));

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items[0].EffectiveFrom.Should().Be(newerDate);
        result.Items[0].BaseDailySalary.Should().Be(25_000_000m);
        result.Items[1].EffectiveFrom.Should().Be(olderDate);
        result.Items[1].BaseDailySalary.Should().Be(20_000_000m);
        result.Items.Should().OnlyContain(x => x.EmployeeName == "علی رضایی");
        result.Items.Should().OnlyContain(x => x.PersonalCode == "EMP001");
        result.Items.Should().OnlyContain(x => x.WorkshopName == "کارگاه نمونه");
        result.Items.Should().OnlyContain(x => x.DepartmentName == "بخش تولید");
    }

    [Fact]
    public async Task GetSalaryDecreesAsync_WithEmployeeIdFilter_ShouldReturnOnlyThatEmployee()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee1 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");
        var employee2 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "مینا احمدی", "0987654321");

        await SaveSalaryProfileAsync(scope, employee1, employee1.HireDate.AddDays(1), 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee2, employee2.HireDate.AddDays(1), 21_000_000m);

        var result = await query.GetSalaryDecreesAsync(
            userId, new PaginationDto(1, 10), employeeId: employee1.Id);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().EmployeeId.Should().Be(employee1.Id);
    }

    [Fact]
    public async Task GetSalaryDecreesAsync_WithSearch_ShouldFilterByFullName()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee1 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");
        var employee2 = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "مینا احمدی", "0987654321");

        await SaveSalaryProfileAsync(scope, employee1, employee1.HireDate.AddDays(1), 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee2, employee2.HireDate.AddDays(1), 21_000_000m);

        var result = await query.GetSalaryDecreesAsync(
            userId, new PaginationDto(1, 10), search: "رضا");

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().EmployeeName.Should().Be("علی رضایی");
    }

    [Fact]
    public async Task GetSalaryDecreesAsync_WithStatusFilter_ShouldReturnOnlyRequestedStatus()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var olderDate = employee.HireDate.AddDays(1);
        var newerDate = employee.HireDate.AddDays(2);
        await SaveSalaryProfileAsync(scope, employee, olderDate, 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee, newerDate, 25_000_000m);

        var activeResult = await query.GetSalaryDecreesAsync(
            userId, new PaginationDto(1, 10), status: SalaryDecreeStatus.Active);
        var expiredResult = await query.GetSalaryDecreesAsync(
            userId, new PaginationDto(1, 10), status: SalaryDecreeStatus.Expired);

        activeResult.TotalCount.Should().Be(1);
        activeResult.Items.Should().ContainSingle();
        activeResult.Items.First().EffectiveFrom.Should().Be(newerDate);
        activeResult.Items.First().Status.Should().Be(SalaryDecreeStatus.Active);

        expiredResult.TotalCount.Should().Be(1);
        expiredResult.Items.Should().ContainSingle();
        expiredResult.Items.First().EffectiveFrom.Should().Be(olderDate);
        expiredResult.Items.First().Status.Should().Be(SalaryDecreeStatus.Expired);
    }

    [Fact]
    public async Task GetSalaryDecreesAsync_WithWorkshopAndDepartmentFilter_ShouldFilterResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
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

        var result = await query.GetSalaryDecreesAsync(
            userId, new PaginationDto(1, 10), workshopId: workshop2.Id, departmentId: departmentId2);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().WorkshopName.Should().Be("کارگاه دوم");
        result.Items.First().DepartmentName.Should().Be("بخش اداری");
    }

    [Fact]
    public async Task GetSalaryDecreesAsync_WithPagination_ShouldPaginateCorrectly()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(1), 20_000_000m);
        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(2), 21_000_000m);
        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(3), 22_000_000m);

        var result = await query.GetSalaryDecreesAsync(userId, new PaginationDto(1, 2));

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSalaryDecreesAsync_WithWrongUserId_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(1), 20_000_000m);

        var result = await query.GetSalaryDecreesAsync(anotherUserId, new PaginationDto(1, 10));

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    #endregion

    #region GetSalaryDecreeByIdAsync

    [Fact]
    public async Task GetSalaryDecreeByIdAsync_WhenProfileExists_ShouldReturnFullProfile()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISalaryDecreeRepository>();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
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
            .WithBaseDailySalary(20_000_000m)
            .WithAttractionAllowance(1_000_000m)
            .WithSupervisionAllowance(2_000_000m)
            .WithShiftType(ShiftType.MorningEvening)
            .WithContractType(ContractType.FixedTerm)
            .WithHousingAllowance(1_400_000m)
            .WithFoodAllowance(2_200_000m)
            .WithTransportationAllowanceNet(500_000m)
            .WithKaranehAmountNet(300_000m)
            .WithMaritalStatus(EmployeeMaritalStatus.Married)
            .WithChildrenCount(2)
            .WithIsTaxSubject(true)
            .WithInsuranceNumber("INS-777")
            .WithSocialSecurityContractRow("CTR-22")
            .WithPositionInInsuranceList("مدیر")
            .WithIsSubjectTo7PercentInsurance(false)
            .WithIsSubjectTo20PercentInsurance(true)
            .WithIsSubjectTo3PercentInsurance(true)
            .WithIsSubjectTo4PercentInsurance(true)
            .WithInsuranceCalculationProfile(InsuranceCalculationProfile.MinimumLaborLaw)
            .CreateResult()
            .ShouldBeSuccess();

        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var result = await query.GetSalaryDecreeByIdAsync(userId, salaryProfile.Id);

        result.Should().NotBeNull();
        result!.EmployeeId.Should().Be(employee.Id);
        result.EffectiveFrom.Should().Be(effectiveFrom);
        result.BaseDailySalary.Should().Be(20_000_000m);
        result.AttractionAllowance.Should().Be(1_000_000m);
        result.SupervisionAllowance.Should().Be(2_000_000m);
        result.ShiftType.Should().Be(ShiftType.MorningEvening);
        result.ContractType.Should().Be(ContractType.FixedTerm);
        result.HousingAllowance.Should().Be(1_400_000m);
        result.FoodAllowance.Should().Be(2_200_000m);
        result.TransportationAllowanceNet.Should().Be(500_000m);
        result.KaranehAmountNet.Should().Be(300_000m);
        result.MaritalStatus.Should().Be(EmployeeMaritalStatus.Married);
        result.ChildrenCount.Should().Be(2);
        result.IsTaxSubject.Should().BeTrue();
        result.InsuranceNumber.Should().Be("INS-777");
        result.SocialSecurityContractRow.Should().Be("CTR-22");
        result.PositionInInsuranceList.Should().Be("مدیر");
        result.IsSubjectTo7PercentInsurance.Should().BeFalse();
        result.IsSubjectTo20PercentInsurance.Should().BeTrue();
        result.IsSubjectTo3PercentInsurance.Should().BeTrue();
        result.IsSubjectTo4PercentInsurance.Should().BeTrue();
        result.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.MinimumLaborLaw);
    }

    [Fact]
    public async Task GetSalaryDecreeByIdAsync_WithNullOptionalFields_ShouldReturnNulls()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISalaryDecreeRepository>();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var salaryProfile = CreateSalaryProfile(employee, employee.HireDate.AddDays(1), 20_000_000m);
        (await repository.CreateAsync(salaryProfile)).Should().Be(salaryProfile.Id);

        var result = await query.GetSalaryDecreeByIdAsync(userId, salaryProfile.Id);

        result.Should().NotBeNull();
        result!.AttractionAllowance.Should().BeNull();
        result.SupervisionAllowance.Should().BeNull();
        result.ShiftType.Should().Be(ShiftType.None);
        result.ContractType.Should().Be(ContractType.Permanent);
        result.HousingAllowance.Should().BeNull();
        result.FoodAllowance.Should().BeNull();
        result.TransportationAllowanceNet.Should().BeNull();
        result.KaranehAmountNet.Should().BeNull();
        result.MaritalStatus.Should().Be(EmployeeMaritalStatus.Single);
        result.ChildrenCount.Should().Be(0);
        result.IsTaxSubject.Should().BeTrue();
        result.InsuranceNumber.Should().Be("INS-001");
        result.PositionInInsuranceList.Should().Be("اپراتور");
        result.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.FullLegal);
    }

    [Fact]
    public async Task GetSalaryDecreeByIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);
        var anotherUserId = await CreateUserAsync(scope, "09123456780");
        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");
        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var salaryProfile = await SaveSalaryProfileAsync(scope, employee, employee.HireDate.AddDays(1), 20_000_000m);

        var result = await query.GetSalaryDecreeByIdAsync(anotherUserId, salaryProfile.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSalaryDecreeByIdAsync_WithWrongSalaryProfileId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<ISalaryDecreeQuery>();
        var userId = await CreateUserAsync(scope);

        var result = await query.GetSalaryDecreeByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion
}
