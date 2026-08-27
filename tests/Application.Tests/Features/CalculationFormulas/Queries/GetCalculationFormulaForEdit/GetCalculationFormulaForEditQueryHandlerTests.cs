namespace Application.Tests.Features.CalculationFormulas.Queries.GetCalculationFormulaForEdit;

public class GetCalculationFormulaForEditQueryHandlerTests
{
    private readonly ICalculationFormulaQuery _calculationFormulaQuery;
    private readonly GetCalculationFormulaForEditQueryHandler _handler;

    private static readonly Guid ValidFormulaId = Guid.NewGuid();
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-10));
    private const string ValidExpression = "OvertimeHours * HourlyRate * 1.4";

    public GetCalculationFormulaForEditQueryHandlerTests()
    {
        _calculationFormulaQuery = Substitute.For<ICalculationFormulaQuery>();
        _handler = new GetCalculationFormulaForEditQueryHandler(_calculationFormulaQuery);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnFormulaDetails()
    {
        var query = new GetCalculationFormulaForEditQuery(ValidFormulaId);
        var formula = new CalculationFormulaByIdResult(
            FormulaKey.OvertimePay,
            ValidExpression,
            ValidEffectiveFrom);

        _calculationFormulaQuery.GetCalculationFormulaByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(formula);

        var result = await _handler.Handle(query, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Key.Should().Be(FormulaKey.OvertimePay);
        response.Expression.Should().Be(ValidExpression);
        response.EffectiveFrom.Should().Be(ValidEffectiveFrom);
    }

    [Fact]
    public async Task Handle_WhenFormulaNotFound_ShouldReturnNotFoundFailure()
    {
        var query = new GetCalculationFormulaForEditQuery(ValidFormulaId);

        _calculationFormulaQuery.GetCalculationFormulaByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns((CalculationFormulaByIdResult?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldCallGetCalculationFormulaByIdAsyncOnce()
    {
        var query = new GetCalculationFormulaForEditQuery(ValidFormulaId);

        _calculationFormulaQuery.GetCalculationFormulaByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns((CalculationFormulaByIdResult?)null);

        await _handler.Handle(query, CancellationToken.None);

        await _calculationFormulaQuery.Received(1)
            .GetCalculationFormulaByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>());
    }
}
