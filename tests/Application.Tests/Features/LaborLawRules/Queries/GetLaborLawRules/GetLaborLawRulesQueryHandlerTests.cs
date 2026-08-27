namespace Application.Tests.Features.LaborLawRules.Queries.GetLaborLawRules;

public class GetLaborLawRulesQueryHandlerTests
{
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly GetLaborLawRulesQueryHandler _handler;

    private static readonly PaginationDto ValidPagination = new(1, 10);

    public GetLaborLawRulesQueryHandlerTests()
    {
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _handler = new GetLaborLawRulesQueryHandler(_laborLawRuleQuery);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnPagedResult()
    {
        var query = new GetLaborLawRulesQuery(ValidPagination, LaborLawRuleKey.MinimumMonthlySalary);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var effectiveFrom1 = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var effectiveFrom2 = DateOnly.FromDateTime(DateTime.Now.AddDays(-40));

        var results = new List<LaborLawRuleResult>
        {
            new(id1, LaborLawRuleKey.MinimumMonthlySalary, 103_909_680m, effectiveFrom1),
            new(id2, LaborLawRuleKey.MinimumMonthlySalary, 71_661_840m, effectiveFrom2)
        };
        var pagedResult = new PagedResult<LaborLawRuleResult>(results, 2, 1, 10);

        _laborLawRuleQuery.GetLaborLawRulesAsync(
                ValidPagination,
                LaborLawRuleKey.MinimumMonthlySalary,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Items.Should().HaveCount(2);
        response.TotalCount.Should().Be(2);
        response.Items[0].Id.Should().Be(id1);
        response.Items[0].Value.Should().Be(103_909_680m);
        response.Items[0].EffectiveFrom.Should().Be(effectiveFrom1);
        response.Items[1].Id.Should().Be(id2);
        response.Items[1].Value.Should().Be(71_661_840m);
    }

    [Fact]
    public async Task Handle_WithNoRules_ShouldReturnEmptyPagedResult()
    {
        var query = new GetLaborLawRulesQuery(ValidPagination);

        _laborLawRuleQuery.GetLaborLawRulesAsync(
                ValidPagination,
                null,
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<LaborLawRuleResult>([], 0, 1, 10));

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNullKey_ShouldCallQueryWithNullKey()
    {
        var query = new GetLaborLawRulesQuery(ValidPagination);

        _laborLawRuleQuery.GetLaborLawRulesAsync(
                ValidPagination,
                null,
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<LaborLawRuleResult>([], 0, 1, 10));

        await _handler.Handle(query, CancellationToken.None);

        await _laborLawRuleQuery.Received(1).GetLaborLawRulesAsync(
            ValidPagination,
            null,
            Arg.Any<CancellationToken>());
    }
}
