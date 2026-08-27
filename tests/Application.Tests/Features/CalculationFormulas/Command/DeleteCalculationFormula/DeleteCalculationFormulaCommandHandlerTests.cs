namespace Application.Tests.Features.CalculationFormulas.Command.DeleteCalculationFormula;

public class DeleteCalculationFormulaCommandHandlerTests
{
    private readonly ICalculationFormulaRepository _calculationFormulaRepository;
    private readonly DeleteCalculationFormulaCommandHandler _handler;
    private readonly CalculationFormulaBuilder _builder;

    private static readonly Guid ValidFormulaId = Guid.NewGuid();

    public DeleteCalculationFormulaCommandHandlerTests()
    {
        _calculationFormulaRepository = Substitute.For<ICalculationFormulaRepository>();
        _handler = new DeleteCalculationFormulaCommandHandler(_calculationFormulaRepository);
        _builder = new CalculationFormulaBuilder();
    }

    private CalculationFormula CreateValidFormula()
    {
        return _builder
            .WithId(ValidFormulaId)
            .CreateResult()
            .ShouldBeSuccess();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldDeleteFormulaAndReturnTrue()
    {
        var command = new DeleteCalculationFormulaCommand(ValidFormulaId);
        var formula = CreateValidFormula();

        _calculationFormulaRepository.GetByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(formula);
        _calculationFormulaRepository.DeleteAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        await _calculationFormulaRepository.Received(1).DeleteAsync(ValidFormulaId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFormulaNotFound_ShouldReturnNotFoundFailure()
    {
        var command = new DeleteCalculationFormulaCommand(ValidFormulaId);

        _calculationFormulaRepository.GetByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns((CalculationFormula?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
        await _calculationFormulaRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryDeleteFails_ShouldReturnGeneralFailure()
    {
        var command = new DeleteCalculationFormulaCommand(ValidFormulaId);
        var formula = CreateValidFormula();

        _calculationFormulaRepository.GetByIdAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(formula);
        _calculationFormulaRepository.DeleteAsync(ValidFormulaId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
