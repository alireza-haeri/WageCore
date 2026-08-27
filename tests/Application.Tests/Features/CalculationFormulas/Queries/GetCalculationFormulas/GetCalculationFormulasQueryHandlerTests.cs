namespace Application.Tests.Features.CalculationFormulas.Queries.GetCalculationFormulas;

public class GetCalculationFormulasQueryHandlerTests
{
    private readonly ICalculationFormulaQuery _calculationFormulaQuery;
    private readonly GetCalculationFormulasQueryHandler _handler;

    private static readonly PaginationDto ValidPagination = new(1, 10);
    private const string Expression1 = "Hours * Rate * 1.4";
    private const string Expression2 = "Hours * Rate * 1.5";

    public GetCalculationFormulasQueryHandlerTests()
    {
        _calculationFormulaQuery = Substitute.For<ICalculationFormulaQuery>();
        _handler = new GetCalculationFormulasQueryHandler(_calculationFormulaQuery);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnPagedResult()
    {
        var query = new GetCalculationFormulasQuery(ValidPagination, FormulaKey.OvertimePay);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var effectiveFrom1 = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
        var effectiveFrom2 = DateOnly.FromDateTime(DateTime.Now.AddDays(-40));

        var results = new List<CalculationFormulaResult>
        {
            new(id1, FormulaKey.OvertimePay, Expression1, effectiveFrom1),
            new(id2, FormulaKey.OvertimePay, Expression2, effectiveFrom2)
        };
        var pagedResult = new PagedResult<CalculationFormulaResult>(results, 2, 1, 10);

        _calculationFormulaQuery.GetCalculationFormulasAsync(
                ValidPagination,
                FormulaKey.OvertimePay,
                Arg.Any<CancellationToken>())
            .Returns(pagedResult);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Items.Should().HaveCount(2);
        response.TotalCount.Should().Be(2);
        response.Items[0].Id.Should().Be(id1);
        response.Items[0].Expression.Should().Be(Expression1);
        response.Items[0].EffectiveFrom.Should().Be(effectiveFrom1);
        response.Items[1].Id.Should().Be(id2);
        response.Items[1].Expression.Should().Be(Expression2);
    }

    [Fact]
    public async Task Handle_WithNoFormulas_ShouldReturnEmptyPagedResult()
    {
        var query = new GetCalculationFormulasQuery(ValidPagination);

        _calculationFormulaQuery.GetCalculationFormulasAsync(
                ValidPagination,
                null,
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<CalculationFormulaResult>([], 0, 1, 10));

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithNullKey_ShouldCallQueryWithNullKey()
    {
        var query = new GetCalculationFormulasQuery(ValidPagination);

        _calculationFormulaQuery.GetCalculationFormulasAsync(
                ValidPagination,
                null,
                Arg.Any<CancellationToken>())
            .Returns(new PagedResult<CalculationFormulaResult>([], 0, 1, 10));

        await _handler.Handle(query, CancellationToken.None);

        await _calculationFormulaQuery.Received(1).GetCalculationFormulasAsync(
            ValidPagination,
            null,
            Arg.Any<CancellationToken>());
    }
}
