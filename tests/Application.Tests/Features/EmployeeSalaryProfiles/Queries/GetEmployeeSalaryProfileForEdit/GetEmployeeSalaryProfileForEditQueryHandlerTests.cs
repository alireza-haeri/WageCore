namespace Application.Tests.Features.EmployeeSalaryProfiles.Queries.GetEmployeeSalaryProfileForEdit;

public class GetEmployeeSalaryProfileForEditQueryHandlerTests
{
    private readonly IEmployeeSalaryProfileQuery _employeeSalaryProfileQuery;
    private readonly GetEmployeeSalaryProfileForEditQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    public GetEmployeeSalaryProfileForEditQueryHandlerTests()
    {
        _employeeSalaryProfileQuery = Substitute.For<IEmployeeSalaryProfileQuery>();
        _handler = new GetEmployeeSalaryProfileForEditQueryHandler(_employeeSalaryProfileQuery);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSalaryProfileDetails()
    {
        var query = new GetEmployeeSalaryProfileForEditQuery(ValidUserId, ValidSalaryProfileId);

        var salaryProfile = new EmployeeSalaryProfileByIdResult(
            ValidEmployeeId,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            71_661_840m,
            1_000_000m,
            2_000_000m,
            SeniorityBaseApplicationMode.Automatic,
            SeniorityBaseCalculationMethod.Daily,
            YearEndSeniorityMode.AnnualLumpSum,
            ShiftType.MorningEveningNight,
            3_000_000m,
            4_000_000m,
            500_000m,
            800_000m,
            1_200_000m);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfileByIdAsync(
                ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(salaryProfile);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.EmployeeSalaryProfileId.Should().Be(ValidSalaryProfileId);
            response.EmployeeId.Should().Be(ValidEmployeeId);
            response.EffectiveFrom.Should().Be(salaryProfile.EffectiveFrom);
            response.BaseMonthlySalary.Should().Be(71_661_840m);
            response.AttractionAllowance.Should().Be(1_000_000m);
            response.SupervisionAllowance.Should().Be(2_000_000m);
            response.SeniorityBaseApplicationMode.Should().Be(SeniorityBaseApplicationMode.Automatic);
            response.SeniorityBaseCalculationMethod.Should().Be(SeniorityBaseCalculationMethod.Daily);
            response.YearEndSeniorityMode.Should().Be(YearEndSeniorityMode.AnnualLumpSum);
            response.ShiftType.Should().Be(ShiftType.MorningEveningNight);
            response.HousingAllowance.Should().Be(3_000_000m);
            response.FoodAllowance.Should().Be(4_000_000m);
            response.ChildAllowancePerChild.Should().Be(500_000m);
            response.TransportationAllowanceNet.Should().Be(800_000m);
            response.KaranehAmountNet.Should().Be(1_200_000m);
        }
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileNotFound_ShouldReturnNotFoundFailure()
    {
        var query = new GetEmployeeSalaryProfileForEditQuery(ValidUserId, ValidSalaryProfileId);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfileByIdAsync(
                ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((EmployeeSalaryProfileByIdResult?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldCallGetEmployeeSalaryProfileByIdAsyncOnce()
    {
        var query = new GetEmployeeSalaryProfileForEditQuery(ValidUserId, ValidSalaryProfileId);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfileByIdAsync(
                ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((EmployeeSalaryProfileByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetEmployeeSalaryProfileByIdAsync(
            ValidUserId,
            ValidSalaryProfileId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToQuery()
    {
        var query = new GetEmployeeSalaryProfileForEditQuery(ValidUserId, ValidSalaryProfileId);

        _employeeSalaryProfileQuery.GetEmployeeSalaryProfileByIdAsync(
                ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((EmployeeSalaryProfileByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _employeeSalaryProfileQuery.Received(1).GetEmployeeSalaryProfileByIdAsync(
            ValidUserId,
            ValidSalaryProfileId,
            Arg.Any<CancellationToken>());
    }
}
