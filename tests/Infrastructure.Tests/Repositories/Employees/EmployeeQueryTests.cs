namespace Infrastructure.Tests.Repositories.Employees;

public class EmployeeQueryTests(WageCoreDbContextFixture fixture)
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
        string workshopName, string departmentName, string nationalId)
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

    private async Task<Core.Domain.Employee> CreateEmployeeAsync(AsyncServiceScope scope, Guid workshopId, Guid departmentId,
        string personalCode, string fullName, string nationalCode, string? jobTitle = "حسابدار")
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
            .WithJobTitle(jobTitle)
            .CreateResult()
            .ShouldBeSuccess();

        var result = await repository.CreateAsync(employee);
        result.Should().Be(employee.Id);

        return employee;
    }

    #region GetUserEmployeesAsync

    [Fact]
    public async Task GetUserEmployeesAsync_WithoutFilters_ShouldReturnAllUserEmployees()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId1 = Guid.NewGuid();
        var workshop1 = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId1,
            "کارگاه اول", "بخش تولید", "1111111111");
        var departmentId2 = Guid.NewGuid();
        var workshop2 = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId2,
            "کارگاه دوم", "بخش اداری", "2222222222");

        await CreateEmployeeAsync(scope, workshop1.Id, departmentId1, "EMP001", "علی رضایی", "1234567890");
        await CreateEmployeeAsync(scope, workshop2.Id, departmentId2, "EMP002", "مینا احمدی", "0987654321");

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserEmployeesAsync(userId, pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(x => x.PersonalCode == "EMP001" && x.FullName == "علی رضایی");
        result.Items.Should().Contain(x => x.PersonalCode == "EMP002" && x.FullName == "مینا احمدی");
        result.Items.Should().Contain(x => x.WorkshopName == "کارگاه اول" && x.DepartmentName == "بخش تولید");
        result.Items.Should().Contain(x => x.WorkshopName == "کارگاه دوم" && x.DepartmentName == "بخش اداری");
    }

    [Fact]
    public async Task GetUserEmployeesAsync_WithFullNameSearch_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "رضا محمدی", "1234567890");
        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "مینا احمدی", "0987654321");

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserEmployeesAsync(userId, pagination, search: "رضا");

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().FullName.Should().Be("رضا محمدی");
    }

    [Fact]
    public async Task GetUserEmployeesAsync_WithPersonalCodeSearch_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP500", "رضا محمدی", "1234567890");
        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP600", "مینا احمدی", "0987654321");

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserEmployeesAsync(userId, pagination, search: "EMP500");

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().PersonalCode.Should().Be("EMP500");
    }

    [Fact]
    public async Task GetUserEmployeesAsync_WithNationalCodeSearch_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "رضا محمدی", "1231231231");
        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "مینا احمدی", "0987654321");

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserEmployeesAsync(userId, pagination, search: "1231231231");

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().NationalCode.Should().Be("1231231231");
    }

    [Fact]
    public async Task GetUserEmployeesAsync_WithWorkshopAndDepartmentFilters_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId1 = Guid.NewGuid();
        var workshop1 = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId1,
            "کارگاه اول", "بخش تولید", "1111111111");
        var departmentId2 = Guid.NewGuid();
        var workshop2 = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId2,
            "کارگاه دوم", "بخش اداری", "2222222222");

        await CreateEmployeeAsync(scope, workshop1.Id, departmentId1, "EMP001", "علی رضایی", "1234567890");
        await CreateEmployeeAsync(scope, workshop2.Id, departmentId2, "EMP002", "مینا احمدی", "0987654321");

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserEmployeesAsync(userId, pagination, workshopId: workshop2.Id,
            departmentId: departmentId2);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().WorkshopName.Should().Be("کارگاه دوم");
        result.Items.First().DepartmentName.Should().Be("بخش اداری");
    }

    [Fact]
    public async Task GetUserEmployeesAsync_WithStatusFilter_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");
        var terminatedEmployee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP002", "مینا احمدی",
            "0987654321");
        terminatedEmployee.Terminate(DateOnly.FromDateTime(DateTime.Now)).ShouldBeSuccess();
        await repository.UpdateAsync(terminatedEmployee);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserEmployeesAsync(userId, pagination, status: EmployeeStatus.Unemployed);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().PersonalCode.Should().Be("EMP002");
        result.Items.First().Status.Should().Be(EmployeeStatus.Unemployed);
    }

    [Fact]
    public async Task GetUserEmployeesAsync_WithWrongUserId_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserEmployeesAsync(Guid.NewGuid(), pagination);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    #endregion

    #region GetUserEmployeeByIdAsync

    [Fact]
    public async Task GetUserEmployeeByIdAsync_WhenEmployeeExists_ShouldReturnEmployeeDetails()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        var employee = _employeeBuilder
            .WithId(Guid.NewGuid())
            .WithWorkshopId(workshop.Id)
            .WithDepartmentId(departmentId)
            .WithPersonalCode("EMP001")
            .WithFullName("علی رضایی")
            .WithNationalCode("1234567890")
            .WithBirthCertificateNumber("54321")
            .WithFatherName("محمد")
            .WithGender(EmployeeGender.Man)
            .WithMaritalStatus(EmployeeMaritalStatus.Married)
            .WithChildrenCount(2)
            .WithWorkshopRegistrationDate(ValidWorkshopRegistrationDate)
            .WithHireDate(DateOnly.FromDateTime(DateTime.Now.AddDays(-10)))
            .WithPhoneNumber("09123456789")
            .WithJobTitle("حسابدار")
            .WithIsTaxSubject(true)
            .WithInsuranceNumber("INS-001")
            .WithSocialSecurityContractRow("CTR-10")
            .WithPositionInInsuranceList("اپراتور")
            .WithIsSubjectTo7PercentInsurance(true)
            .WithIsSubjectTo20PercentInsurance(true)
            .WithIsSubjectTo3PercentInsurance(false)
            .WithInsuranceCalculationProfile(InsuranceCalculationProfile.FullLegal)
            .CreateResult()
            .ShouldBeSuccess();

        employee.ReplaceBankAccounts([
            new EmployeeBankAccountDto("حساب حقوق", "IR123456789012345678901234", Guid.NewGuid()),
            new EmployeeBankAccountDto("حساب پس انداز", "IR999999999999999999999999", Guid.NewGuid())
        ]).ShouldBeSuccess();

        var createResult = await repository.CreateAsync(employee);
        createResult.Should().Be(employee.Id);

        var result = await query.GetUserEmployeeByIdAsync(userId, employee.Id);

        result.Should().NotBeNull();
        result!.WorkshopId.Should().Be(workshop.Id);
        result.DepartmentId.Should().Be(departmentId);
        result.PersonalCode.Should().Be("EMP001");
        result.FullName.Should().Be("علی رضایی");
        result.NationalCode.Should().Be("1234567890");
        result.BirthCertificateNumber.Should().Be("54321");
        result.FatherName.Should().Be("محمد");
        result.Gender.Should().Be(EmployeeGender.Man);
        result.MaritalStatus.Should().Be(EmployeeMaritalStatus.Married);
        result.ChildrenCount.Should().Be(2);
        result.HireDate.Should().Be(employee.HireDate);
        result.PhoneNumber.Should().Be("09123456789");
        result.JobTitle.Should().Be("حسابدار");
        result.IsTaxSubject.Should().BeTrue();
        result.InsuranceNumber.Should().Be("INS-001");
        result.SocialSecurityContractRow.Should().Be("CTR-10");
        result.PositionInInsuranceList.Should().Be("اپراتور");
        result.IsSubjectTo7PercentInsurance.Should().BeTrue();
        result.IsSubjectTo20PercentInsurance.Should().BeTrue();
        result.IsSubjectTo3PercentInsurance.Should().BeFalse();
        result.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.FullLegal);
        result.BankAccounts.Should().HaveCount(2);
        result.BankAccounts.Should().Contain(x => x.Title == "حساب حقوق" && x.Iban == "123456789012345678901234" && x.Id.HasValue);
        result.BankAccounts.Should().Contain(x => x.Title == "حساب پس انداز" && x.Iban == "999999999999999999999999" && x.Id.HasValue);
    }

    [Fact]
    public async Task GetUserEmployeeByIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var result = await query.GetUserEmployeeByIdAsync(Guid.NewGuid(), employee.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserEmployeeByIdAsync_WithWrongEmployeeId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var result = await query.GetUserEmployeeByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserEmployeeByIdAsync_WithNullOptionalFields_ShouldReturnNullOptionalFields()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        var employee = _employeeBuilder
            .WithId(Guid.NewGuid())
            .WithWorkshopId(workshop.Id)
            .WithDepartmentId(departmentId)
            .WithWorkshopRegistrationDate(ValidWorkshopRegistrationDate)
            .WithJobTitle(null)
            .WithSocialSecurityContractRow(null)
            .WithMaritalStatus(EmployeeMaritalStatus.Single)
            .WithChildrenCount(0)
            .CreateResult()
            .ShouldBeSuccess();

        var createResult = await repository.CreateAsync(employee);
        createResult.Should().Be(employee.Id);

        var result = await query.GetUserEmployeeByIdAsync(userId, employee.Id);

        result.Should().NotBeNull();
        result!.JobTitle.Should().BeNull();
        result.SocialSecurityContractRow.Should().BeNull();
        result.MaritalStatus.Should().Be(EmployeeMaritalStatus.Single);
        result.ChildrenCount.Should().Be(0);
        result.BankAccounts.Should().BeEmpty();
    }

    #endregion

    #region IsExistEmployeePersonalCode

    [Fact]
    public async Task IsExistEmployeePersonalCode_WhenPersonalCodeExists_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var result = await query.IsExistEmployeePersonalCode(userId, "EMP001");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistEmployeePersonalCode_WithExcludeEmployeeId_ShouldExcludeThatEmployee()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var result = await query.IsExistEmployeePersonalCode(userId, "EMP001", employee.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistEmployeePersonalCode_WhenPersonalCodeExistsForAnotherUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var user1Id = await CreateUserAsync(scope);
        var user2Id = await CreateUserAsync(scope, "09123456780");

        var departmentId1 = Guid.NewGuid();
        var workshop1 = await CreateWorkshopWithDepartmentAsync(scope, user1Id, departmentId1,
            "کارگاه اول", "بخش تولید", "1111111111");
        var departmentId2 = Guid.NewGuid();
        await CreateWorkshopWithDepartmentAsync(scope, user2Id, departmentId2,
            "کارگاه دوم", "بخش اداری", "2222222222");

        await CreateEmployeeAsync(scope, workshop1.Id, departmentId1, "EMP001", "علی رضایی", "1234567890");

        var result = await query.IsExistEmployeePersonalCode(user2Id, "EMP001");

        result.Should().BeFalse();
    }

    #endregion

    #region IsExistEmployeeNationalCode

    [Fact]
    public async Task IsExistEmployeeNationalCode_WhenNationalCodeExists_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var result = await query.IsExistEmployeeNationalCode(userId, "1234567890");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistEmployeeNationalCode_WithExcludeEmployeeId_ShouldExcludeThatEmployee()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");

        var result = await query.IsExistEmployeeNationalCode(userId, "1234567890", employee.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistEmployeeNationalCode_WhenNationalCodeExistsForAnotherUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var user1Id = await CreateUserAsync(scope);
        var user2Id = await CreateUserAsync(scope, "09123456780");

        var departmentId1 = Guid.NewGuid();
        var workshop1 = await CreateWorkshopWithDepartmentAsync(scope, user1Id, departmentId1,
            "کارگاه اول", "بخش تولید", "1111111111");
        var departmentId2 = Guid.NewGuid();
        await CreateWorkshopWithDepartmentAsync(scope, user2Id, departmentId2,
            "کارگاه دوم", "بخش اداری", "2222222222");

        await CreateEmployeeAsync(scope, workshop1.Id, departmentId1, "EMP001", "علی رضایی", "1234567890");

        var result = await query.IsExistEmployeeNationalCode(user2Id, "1234567890");

        result.Should().BeFalse();
    }

    #endregion

    [Fact]
    public async Task GetUserEmployeesAsync_WhenEmployeeIsRehired_ShouldReturnEmployedStatus()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<EmployeeRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IEmployeeQuery>();
        var userId = await CreateUserAsync(scope);

        var departmentId = Guid.NewGuid();
        var workshop = await CreateWorkshopWithDepartmentAsync(scope, userId, departmentId,
            "کارگاه نمونه", "بخش تولید", "1111111111");

        var employee = await CreateEmployeeAsync(scope, workshop.Id, departmentId, "EMP001", "علی رضایی", "1234567890");
        var rehireDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        employee.Terminate(DateOnly.FromDateTime(DateTime.Now.AddDays(-3))).ShouldBeSuccess();
        await repository.UpdateAsync(employee);

        employee.Rehire(new EmployeeRehireDto(
            departmentId,
            workshop.RegistrationDate,
            rehireDate)).ShouldBeSuccess();
        await repository.UpdateAsync(employee);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserEmployeesAsync(userId, pagination, status: EmployeeStatus.Employed);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().PersonalCode.Should().Be("EMP001");
        result.Items.First().Status.Should().Be(EmployeeStatus.Employed);
        result.Items.First().HireDate.Should().Be(rehireDate);
    }
}
