namespace Infrastructure.Tests.Repositories.Workshops;

public class WorkshopQueryTests(WageCoreDbContextFixture fixture)
    : IClassFixture<WageCoreDbContextFixture>, IAsyncLifetime
{
    private readonly WorkshopBuilder _workshopBuilder = new();

    public async Task InitializeAsync() => await fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<Guid> CreateUserAsync(AsyncServiceScope scope,string? phoneNumber = "09123456789")
    {
        var userRepository = scope.ServiceProvider.GetRequiredService<UserRepository>();
        var user = new UserBuilder().WithPhoneNumber(phoneNumber).CreateResult().ShouldBeSuccess();
        var result = await userRepository.CreateAsync(user, "Pass123456");
        result.Succeeded.Should().BeTrue();
        return user.Id;
    }

    #region GetUserWorkshopsAsync

    [Fact]
    public async Task GetUserWorkshopsAsync_WithoutFilters_ShouldReturnAllUserWorkshops()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه اول")
            .CreateResult()
            .ShouldBeSuccess();

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه دوم")
            .WithRegion(WorkshopRegion.LessDeveloped)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop1);
        await repository.CreateAsync(workshop2);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserWorkshopsAsync(userId, pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().Contain(w => w.Name == "کارگاه اول");
        result.Items.Should().Contain(w => w.Name == "کارگاه دوم");
    }

    [Fact]
    public async Task GetUserWorkshopsAsync_WithSearchName_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = _workshopBuilder
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        var workshop2 = _workshopBuilder
            .WithUserId(userId)
            .WithName("تراشکاری نوین")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop1);
        await repository.CreateAsync(workshop2);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserWorkshopsAsync(userId, pagination, searchName: "آریا");

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("کارگاه آریا");
    }

    [Fact]
    public async Task GetUserWorkshopsAsync_WithRegionFilter_ShouldReturnFilteredResults()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه عادی")
            .WithRegion(WorkshopRegion.Normal)
            .CreateResult()
            .ShouldBeSuccess();

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه محروم")
            .WithRegion(WorkshopRegion.LessDeveloped)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop1);
        await repository.CreateAsync(workshop2);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserWorkshopsAsync(userId, pagination, region: WorkshopRegion.LessDeveloped);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("کارگاه محروم");
    }

    [Fact]
    public async Task GetUserWorkshopsAsync_WithPagination_ShouldPaginateCorrectly()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        for (var i = 1; i <= 5; i++)
        {
            var workshop = _workshopBuilder
                .WithId(Guid.NewGuid())
                .WithUserId(userId)
                .WithName($"کارگاه شماره {i}")
                .CreateResult()
                .ShouldBeSuccess();

            await repository.CreateAsync(workshop);
        }

        var pagination = new PaginationDto(1, 2);
        var result = await query.GetUserWorkshopsAsync(userId, pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserWorkshopsAsync_WithSecondPage_ShouldReturnCorrectItems()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        for (var i = 1; i <= 5; i++)
        {
            var workshop = _workshopBuilder
                .WithId(Guid.NewGuid())
                .WithUserId(userId)
                .WithName($"کارگاه شماره {i}")
                .CreateResult()
                .ShouldBeSuccess();

            await repository.CreateAsync(workshop);
        }

        var pagination = new PaginationDto(2, 2);
        var result = await query.GetUserWorkshopsAsync(userId, pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(5);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserWorkshopsAsync_WithWrongUserId_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserWorkshopsAsync(Guid.NewGuid(), pagination);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserWorkshopsAsync_WithCombinedFilters_ShouldReturnCorrectResults()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .WithRegion(WorkshopRegion.Normal)
            .CreateResult()
            .ShouldBeSuccess();

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("آریا تراش")
            .WithRegion(WorkshopRegion.LessDeveloped)
            .CreateResult()
            .ShouldBeSuccess();

        var workshop3 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه نوین")
            .WithRegion(WorkshopRegion.Normal)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop1);
        await repository.CreateAsync(workshop2);
        await repository.CreateAsync(workshop3);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserWorkshopsAsync(
            userId, pagination, searchName: "آریا", region: WorkshopRegion.Normal);

        result.Should().NotBeNull();
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Name.Should().Be("کارگاه آریا");
    }

    [Fact]
    public async Task GetUserWorkshopsAsync_ShouldReturnZeroForEmployeesAndDepartmentsCount()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var pagination = new PaginationDto(1, 10);
        var result = await query.GetUserWorkshopsAsync(userId, pagination);

        result.Items.Should().ContainSingle();
        result.Items.First().EmployeesCount.Should().Be(0);
        result.Items.First().DepartmentsCount.Should().Be(0);
    }

    #endregion

    #region GetUserWorkshopsNameAsync

    [Fact]
    public async Task GetUserWorkshopsNameAsync_WhenWorkshopsExist_ShouldReturnNames()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه اول")
            .CreateResult()
            .ShouldBeSuccess();

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه دوم")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop1);
        await repository.CreateAsync(workshop2);

        var result = await query.GetUserWorkshopsNameAsync(userId);

        result.Should().HaveCount(2);
        result.Should().Contain(w => w.DisplayName == "کارگاه اول");
        result.Should().Contain(w => w.DisplayName == "کارگاه دوم");
    }

    [Fact]
    public async Task GetUserWorkshopsNameAsync_WithLongName_ShouldTruncateDisplayName()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var maxLength = Core.Domain.Workshop.MaxDisplayNameLength;
        var longName = new string('ا', maxLength + 20);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .WithName(longName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.GetUserWorkshopsNameAsync(userId);

        result.Should().ContainSingle();
        result.First().DisplayName.Should().EndWith("...");
        result.First().DisplayName.Length.Should().Be(maxLength + 3);
    }

    [Fact]
    public async Task GetUserWorkshopsNameAsync_WithShortName_ShouldNotTruncate()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var shortName = "کارگاه کوتاه";

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .WithName(shortName)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.GetUserWorkshopsNameAsync(userId);

        result.Should().ContainSingle();
        result.First().DisplayName.Should().Be(shortName);
    }

    [Fact]
    public async Task GetUserWorkshopsNameAsync_WithWrongUserId_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.GetUserWorkshopsNameAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserWorkshopsNameAsync_WhenNoWorkshops_ShouldReturnEmpty()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();

        var result = await query.GetUserWorkshopsNameAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserWorkshopByIdAsync

    [Fact]
    public async Task GetUserWorkshopByIdAsync_WhenWorkshopExists_ShouldReturnWorkshop()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .WithAddress("تهران، خیابان اصلی، پلاک ۱۰")
            .WithRegion(WorkshopRegion.LessDeveloped)
            .WithNationalId("1234567890")
            .WithPostalCode("0987654321")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.GetUserWorkshopByIdAsync(userId, workshop.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("کارگاه آریا");
        result.Address.Should().Be("تهران، خیابان اصلی، پلاک ۱۰");
        result.Region.Should().Be(WorkshopRegion.LessDeveloped);
        result.RegistrationDate.Should().Be(workshop.RegistrationDate);
        result.NationalId.Should().Be("1234567890");
        result.PostalCode.Should().Be("0987654321");
    }

    [Fact]
    public async Task GetUserWorkshopByIdAsync_WithNullPostalCode_ShouldReturnNullPostalCode()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .WithPostalCode(null)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.GetUserWorkshopByIdAsync(userId, workshop.Id);

        result.Should().NotBeNull();
        result!.PostalCode.Should().BeNull();
    }

    [Fact]
    public async Task GetUserWorkshopByIdAsync_WhenWorkshopDoesNotExist_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var result = await query.GetUserWorkshopByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserWorkshopByIdAsync_WithWrongUserId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.GetUserWorkshopByIdAsync(Guid.NewGuid(), workshop.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserWorkshopByIdAsync_WithWrongWorkshopId_ShouldReturnNull()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithUserId(userId)
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.GetUserWorkshopByIdAsync(userId, Guid.NewGuid());

        result.Should().BeNull();
    }

    #endregion

    #region IsExistWorkshopName

    [Fact]
    public async Task IsExistWorkshopName_WhenNameExists_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopName(userId, "کارگاه آریا");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopName_WhenNameDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopName(userId, "کارگاه دیگر");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistWorkshopName_WhenNoWorkshopsExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();

        var result = await query.IsExistWorkshopName(Guid.NewGuid(), "کارگاه آریا");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistWorkshopName_WithDifferentCase_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("Workshop ARIA")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopName(userId, "workshop aria");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopName_WithLeadingOrTrailingSpaces_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopName(userId, "   کارگاه آریا   ");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopName_WithExcludeWorkshopId_ShouldExcludeThatWorkshop()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopName(userId, "کارگاه آریا", excludeWorkshopId: workshop.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistWorkshopName_WithExcludeWorkshopIdAndOtherDuplicate_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop1);
        await repository.CreateAsync(workshop2);

        var result = await query.IsExistWorkshopName(userId, "کارگاه آریا", excludeWorkshopId: workshop1.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopName_WithNullExcludeId_ShouldCheckAllWorkshops()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopName(userId, "کارگاه آریا", excludeWorkshopId: null);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopName_WhenNameExistsForAnotherUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();

        var user1Id = await CreateUserAsync(scope);
        var user2Id = await CreateUserAsync(scope,"09123456780");

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(user1Id)
            .WithName("کارگاه آریا")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopName(user2Id, "کارگاه آریا");

        result.Should().BeFalse();
    }

    #endregion

    #region IsExistWorkshopNationalId

    [Fact]
    public async Task IsExistWorkshopNationalId_WhenNationalIdExists_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithNationalId("1234567890")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopNationalId(userId, "1234567890");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopNationalId_WhenNationalIdDoesNotExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithNationalId("1234567890")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopNationalId(userId, "98765432100");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistWorkshopNationalId_WhenNoWorkshopsExist_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();

        var result = await query.IsExistWorkshopNationalId(Guid.NewGuid(), "1234567890");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistWorkshopNationalId_WithLeadingOrTrailingSpaces_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithNationalId("1234567890")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopNationalId(userId, "   1234567890   ");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopNationalId_WithExcludeWorkshopId_ShouldExcludeThatWorkshop()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithNationalId("1234567890")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopNationalId(userId, "1234567890", excludeWorkshopId: workshop.Id);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsExistWorkshopNationalId_WithExcludeWorkshopIdAndOtherDuplicate_ShouldReturnTrue()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop1 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithNationalId("1234567890")
            .CreateResult()
            .ShouldBeSuccess();

        var workshop2 = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithNationalId("1234567890")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop1);
        await repository.CreateAsync(workshop2);

        var result = await query.IsExistWorkshopNationalId(userId, "1234567890", excludeWorkshopId: workshop1.Id);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopNationalId_WithNullExcludeId_ShouldCheckAllWorkshops()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();
        var userId = await CreateUserAsync(scope);

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(userId)
            .WithNationalId("1234567890")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopNationalId(userId, "1234567890", excludeWorkshopId: null);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsExistWorkshopNationalId_WhenNationalIdExistsForAnotherUser_ShouldReturnFalse()
    {
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<WorkshopRepository>();
        var query = scope.ServiceProvider.GetRequiredService<IWorkshopQuery>();

        var user1Id = await CreateUserAsync(scope);
        var user2Id = await CreateUserAsync(scope,"09123456780");

        var workshop = _workshopBuilder
            .WithId(Guid.NewGuid())
            .WithUserId(user1Id)
            .WithNationalId("1234567890")
            .CreateResult()
            .ShouldBeSuccess();

        await repository.CreateAsync(workshop);

        var result = await query.IsExistWorkshopNationalId(user2Id, "1234567890");

        result.Should().BeFalse();
    }

    #endregion
}