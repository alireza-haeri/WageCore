namespace Application.Tests.Features.LaborLawRules.Queries.GetLaborLawRuleForEdit;

public class GetLaborLawRuleForEditQueryHandlerTests
{
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly GetLaborLawRuleForEditQueryHandler _handler;

    private static readonly Guid ValidRuleId = Guid.NewGuid();
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));

    public GetLaborLawRuleForEditQueryHandlerTests()
    {
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _handler = new GetLaborLawRuleForEditQueryHandler(_laborLawRuleQuery);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnRuleDetails()
    {
        var query = new GetLaborLawRuleForEditQuery(ValidRuleId);
        var rule = new LaborLawRuleByIdResult(
            LaborLawRuleKey.MinimumMonthlySalary,
            103_909_680m,
            ValidEffectiveFrom);

        _laborLawRuleQuery.GetLaborLawRuleByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(rule);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Key.Should().Be(LaborLawRuleKey.MinimumMonthlySalary);
        response.Value.Should().Be(103_909_680m);
        response.EffectiveFrom.Should().Be(ValidEffectiveFrom);
    }

    [Fact]
    public async Task Handle_WhenRuleNotFound_ShouldReturnNotFoundFailure()
    {
        var query = new GetLaborLawRuleForEditQuery(ValidRuleId);

        _laborLawRuleQuery.GetLaborLawRuleByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns((LaborLawRuleByIdResult?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldCallGetLaborLawRuleByIdAsyncOnce()
    {
        var query = new GetLaborLawRuleForEditQuery(ValidRuleId);

        _laborLawRuleQuery.GetLaborLawRuleByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns((LaborLawRuleByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _laborLawRuleQuery.Received(1)
            .GetLaborLawRuleByIdAsync(ValidRuleId, Arg.Any<CancellationToken>());
    }
}
