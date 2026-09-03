namespace Application.Tests.Features.CalculationFormulas.Command.CreateCalculationFormula;

public class CreateCalculationFormulaCommandHandlerTests
{
    private readonly ICalculationFormulaRepository _calculationFormulaRepository;
    private readonly ICalculationFormulaQuery _calculationFormulaQuery;
    private readonly CreateCalculationFormulaCommandHandler _handler;

    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const string ValidExpression = "OvertimeHours * HourlyRate * 1.4";

    public CreateCalculationFormulaCommandHandlerTests()
    {
        _calculationFormulaRepository = Substitute.For<ICalculationFormulaRepository>();
        _calculationFormulaQuery = Substitute.For<ICalculationFormulaQuery>();
        _handler = new CreateCalculationFormulaCommandHandler(
            _calculationFormulaRepository,
            _calculationFormulaQuery);
    }

    private static CreateCalculationFormulaCommand CreateValidCommand(
        FormulaKey? key = FormulaKey.OvertimePay,
        string expression = ValidExpression,
        DateOnly? effectiveFrom = null) =>
        new(key, expression, effectiveFrom ?? ValidEffectiveFrom);

    private void SetupNoDuplicateEffectiveFrom()
    {
        _calculationFormulaQuery.IsExistEffectiveFrom(
                Arg.Any<FormulaKey>(),
                Arg.Any<DateOnly>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateFormulaAndReturnId()
    {
        var command = CreateValidCommand();
        var createdId = Guid.NewGuid();

        SetupNoDuplicateEffectiveFrom();
        _calculationFormulaRepository.CreateAsync(Arg.Any<CalculationFormula>(), Arg.Any<CancellationToken>())
            .Returns(createdId);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.CalculationFormulaId.Should().Be(createdId);

        await _calculationFormulaRepository.Received(1).CreateAsync(
            Arg.Is<CalculationFormula>(x =>
                x.Key == FormulaKey.OvertimePay &&
                x.Expression == ValidExpression &&
                x.EffectiveFrom == ValidEffectiveFrom),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromAlreadyExists_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();

        _calculationFormulaQuery.IsExistEffectiveFrom(
                FormulaKey.OvertimePay,
                ValidEffectiveFrom,
                null,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اجرا تکراری است.", BadResultType.Validation);
        await _calculationFormulaRepository.DidNotReceive()
            .CreateAsync(Arg.Any<CalculationFormula>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(expression: " ");

        SetupNoDuplicateEffectiveFrom();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("عبارت فرمول نمیتواند خالی باشد.", BadResultType.General);
        await _calculationFormulaRepository.DidNotReceive()
            .CreateAsync(Arg.Any<CalculationFormula>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromIsNull_ShouldReturnGeneralFailure()
    {
        var command = new CreateCalculationFormulaCommand(
            FormulaKey.OvertimePay,
            ValidExpression,
            null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اجرا نمیتواند خالی باشد.", BadResultType.General);
        await _calculationFormulaRepository.DidNotReceive()
            .CreateAsync(Arg.Any<CalculationFormula>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryCreateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();

        SetupNoDuplicateEffectiveFrom();
        _calculationFormulaRepository.CreateAsync(Arg.Any<CalculationFormula>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
