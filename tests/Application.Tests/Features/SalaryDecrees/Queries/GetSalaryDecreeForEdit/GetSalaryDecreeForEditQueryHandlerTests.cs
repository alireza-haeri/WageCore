namespace Application.Tests.Features.SalaryDecrees.Queries.GetSalaryDecreeForEdit;

public class GetSalaryDecreeForEditQueryHandlerTests
{
    private readonly ISalaryDecreeQuery _salaryDecreeQuery;
    private readonly GetSalaryDecreeForEditQueryHandler _handler;

    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly Guid ValidSalaryProfileId = Guid.NewGuid();
    private static readonly Guid ValidEmployeeId = Guid.NewGuid();

    public GetSalaryDecreeForEditQueryHandlerTests()
    {
        _salaryDecreeQuery = Substitute.For<ISalaryDecreeQuery>();
        _handler = new GetSalaryDecreeForEditQueryHandler(_salaryDecreeQuery);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSalaryProfileDetails()
    {
        var query = new GetSalaryDecreeForEditQuery(ValidUserId, ValidSalaryProfileId);

        var salaryProfile = new SalaryDecreeByIdResult(
            ValidEmployeeId,
            DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            71_661_840m,
            1_000_000m,
            2_000_000m,
            ShiftType.MorningEveningNight,
            ContractType.FixedTerm,
            800_000m,
            EmployeeMaritalStatus.Married,
            2,
            true,
            "INS-001",
            "اپراتور",
            true,
            true,
            false,
            true,
            InsuranceCalculationProfile.FullLegal);

        _salaryDecreeQuery.GetSalaryDecreeByIdAsync(
                ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns(salaryProfile);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        using (new AssertionScope())
        {
            response.SalaryDecreeId.Should().Be(ValidSalaryProfileId);
            response.EmployeeId.Should().Be(ValidEmployeeId);
            response.EffectiveFrom.Should().Be(salaryProfile.EffectiveFrom);
            response.BaseDailySalary.Should().Be(71_661_840m);
            response.AttractionAllowance.Should().Be(1_000_000m);
            response.SupervisionAllowance.Should().Be(2_000_000m);
            response.ShiftType.Should().Be(ShiftType.MorningEveningNight);
            response.ContractType.Should().Be(ContractType.FixedTerm);
            response.TransportationAllowanceNet.Should().Be(800_000m);
            response.MaritalStatus.Should().Be(EmployeeMaritalStatus.Married);
            response.ChildrenCount.Should().Be(2);
            response.IsTaxSubject.Should().BeTrue();
            response.InsuranceNumber.Should().Be("INS-001");
            response.PositionInInsuranceList.Should().Be("اپراتور");
            response.IsSubjectTo7PercentInsurance.Should().BeTrue();
            response.IsSubjectTo20PercentInsurance.Should().BeTrue();
            response.IsSubjectTo3PercentInsurance.Should().BeFalse();
            response.IsSubjectTo4PercentInsurance.Should().BeTrue();
            response.InsuranceCalculationProfile.Should().Be(InsuranceCalculationProfile.FullLegal);
        }
    }

    [Fact]
    public async Task Handle_WhenSalaryProfileNotFound_ShouldReturnNotFoundFailure()
    {
        var query = new GetSalaryDecreeForEditQuery(ValidUserId, ValidSalaryProfileId);

        _salaryDecreeQuery.GetSalaryDecreeByIdAsync(
                ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((SalaryDecreeByIdResult?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeFailure("پروفایل حقوق کارمند مورد نظر یافت نشد.", BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldCallGetSalaryDecreeByIdAsyncOnce()
    {
        var query = new GetSalaryDecreeForEditQuery(ValidUserId, ValidSalaryProfileId);

        _salaryDecreeQuery.GetSalaryDecreeByIdAsync(
                ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((SalaryDecreeByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreeByIdAsync(
            ValidUserId,
            ValidSalaryProfileId,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectParametersToQuery()
    {
        var query = new GetSalaryDecreeForEditQuery(ValidUserId, ValidSalaryProfileId);

        _salaryDecreeQuery.GetSalaryDecreeByIdAsync(
                ValidUserId, ValidSalaryProfileId, Arg.Any<CancellationToken>())
            .Returns((SalaryDecreeByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _salaryDecreeQuery.Received(1).GetSalaryDecreeByIdAsync(
            ValidUserId,
            ValidSalaryProfileId,
            Arg.Any<CancellationToken>());
    }
}
