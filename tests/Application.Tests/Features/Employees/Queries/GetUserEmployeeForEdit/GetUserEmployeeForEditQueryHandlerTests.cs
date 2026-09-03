namespace Application.Tests.Features.Employees.Queries.GetUserEmployeeForEdit;

public class GetUserEmployeeForEditQueryHandlerTests
{
    private readonly IEmployeeQuery _employeeQuery;
    private readonly GetUserEmployeeForEditQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();
    private static readonly Guid ValidWorkshopId = Guid.NewGuid();
    private static readonly Guid ValidDepartmentId = Guid.NewGuid();
    private static readonly Guid ValidBankAccountId = Guid.NewGuid();

    public GetUserEmployeeForEditQueryHandlerTests()
    {
        _employeeQuery = Substitute.For<IEmployeeQuery>();
        _handler = new GetUserEmployeeForEditQueryHandler(_employeeQuery);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnEmployeeDetails()
    {
        var query = new GetUserEmployeeForEditQuery(ValidUserId, ValidEmployeeId);

        var employee = new UserEmployeeByIdResult(
            ValidWorkshopId,
            ValidDepartmentId,
            "EMP001",
            "علی رضایی",
            "1234567890",
            "محمد",
            EmployeeGender.Man,
            DateOnly.FromDateTime(DateTime.Today),
            "09123456789",
            "حسابدار",
            Region.Normal,
            3,
            45,
            5,
            [
                new EmployeeBankAccountDto("بانک ملی", "۱۰۲", "123456789012345678901234", ValidBankAccountId),
                new EmployeeBankAccountDto("بانک صادرات", "۳۰۳", "999999999999999999999999", Guid.NewGuid())
            ]);

        _employeeQuery.GetUserEmployeeByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.WorkshopId.Should().Be(ValidWorkshopId);
        response.DepartmentId.Should().Be(ValidDepartmentId);
        response.PersonalCode.Should().Be("EMP001");
        response.FullName.Should().Be("علی رضایی");
        response.NationalCode.Should().Be("1234567890");
        response.FatherName.Should().Be("محمد");
        response.Gender.Should().Be(EmployeeGender.Man);
        response.HireDate.Should().Be(DateOnly.FromDateTime(DateTime.Today));
        response.PhoneNumber.Should().Be("09123456789");
        response.JobTitle.Should().Be("حسابدار");
        response.Region.Should().Be(Region.Normal);
        response.LeaveUsedInCurrentYear.Should().Be(3);
        response.NetWorkedDaysBeforeCurrentMonth.Should().Be(45);
        response.CarriedOverLeaveFromPreviousYear.Should().Be(5);
        response.BankAccounts.Should().HaveCount(2);
        response.BankAccounts.Should().Contain(x => x.Id == ValidBankAccountId && x.BankName == "بانک ملی" && x.BranchCode == "۱۰۲" && x.Iban == "123456789012345678901234");
    }

    [Fact]
    public async Task Handle_WhenEmployeeHasNoBankAccount_ShouldReturnEmptyBankAccounts()
    {
        var query = new GetUserEmployeeForEditQuery(ValidUserId, ValidEmployeeId);

        var employee = new UserEmployeeByIdResult(
            ValidWorkshopId,
            ValidDepartmentId,
            "EMP001",
            "علی رضایی",
            "1234567890",
            "محمد",
            EmployeeGender.Man,
            DateOnly.FromDateTime(DateTime.Today),
            "09123456789",
            "حسابدار",
            Region.Normal,
            null,
            null,
            null,
            []);

        _employeeQuery.GetUserEmployeeByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns(employee);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.BankAccounts.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotFoundFailure()
    {
        var query = new GetUserEmployeeForEditQuery(ValidUserId, ValidEmployeeId);

        _employeeQuery.GetUserEmployeeByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((UserEmployeeByIdResult?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeFailure();
    }

    [Fact]
    public async Task Handle_ShouldCallGetUserEmployeeByIdAsyncOnce()
    {
        var query = new GetUserEmployeeForEditQuery(ValidUserId, ValidEmployeeId);

        _employeeQuery.GetUserEmployeeByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((UserEmployeeByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeQuery.Received(1).GetUserEmployeeByIdAsync(
            ValidUserId,
            ValidEmployeeId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToQuery()
    {
        var query = new GetUserEmployeeForEditQuery(ValidUserId, ValidEmployeeId);

        _employeeQuery.GetUserEmployeeByIdAsync(ValidUserId, ValidEmployeeId, Arg.Any<CancellationToken>())
            .Returns((UserEmployeeByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeQuery.Received(1).GetUserEmployeeByIdAsync(
            ValidUserId,
            ValidEmployeeId,
            Arg.Any<CancellationToken>());
    }
}
