namespace Application.Tests.Features.LaborLawRules.Command.CreateLaborLawRule;

public class CreateLaborLawRuleCommandHandlerTests
{
    private readonly ILaborLawRuleRepository _laborLawRuleRepository;
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly CreateLaborLawRuleCommandHandler _handler;

    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const decimal ValidValue = 71_661_840m;

    public CreateLaborLawRuleCommandHandlerTests()
    {
        _laborLawRuleRepository = Substitute.For<ILaborLawRuleRepository>();
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _handler = new CreateLaborLawRuleCommandHandler(_laborLawRuleRepository, _laborLawRuleQuery);
    }

    private static CreateLaborLawRuleCommand CreateValidCommand(
        LaborLawRuleKey? key = LaborLawRuleKey.MinimumDailySalary,
        decimal? value = ValidValue,
        DateOnly? effectiveFrom = null) =>
        new(key, value, effectiveFrom ?? ValidEffectiveFrom);

    private void SetupNoDuplicateEffectiveFrom()
    {
        _laborLawRuleQuery.IsExistEffectiveFrom(
                Arg.Any<DateOnly>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateRuleAndReturnId()
    {
        var command = CreateValidCommand();
        var createdId = Guid.NewGuid();

        SetupNoDuplicateEffectiveFrom();
        _laborLawRuleRepository.CreateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>())
            .Returns(createdId);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.LaborLawRuleId.Should().Be(createdId);

        await _laborLawRuleRepository.Received(1).CreateAsync(
            Arg.Is<LaborLawRuleItem>(x =>
                x.Key == LaborLawRuleKey.MinimumDailySalary &&
                x.Value == ValidValue &&
                x.EffectiveFrom == ValidEffectiveFrom),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromAlreadyExists_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();

        _laborLawRuleQuery.IsExistEffectiveFrom(
                ValidEffectiveFrom,
                null,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اجرا تکراری است.", BadResultType.Validation);
        await _laborLawRuleRepository.DidNotReceive()
            .CreateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(value: -1);

        SetupNoDuplicateEffectiveFrom();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("مقدار قانون نمیتواند منفی باشد.", BadResultType.General);
        await _laborLawRuleRepository.DidNotReceive()
            .CreateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromIsNull_ShouldReturnGeneralFailure()
    {
        var command = new CreateLaborLawRuleCommand(
            LaborLawRuleKey.MinimumDailySalary,
            ValidValue,
            null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اجرا نمیتواند خالی باشد.", BadResultType.General);
        await _laborLawRuleRepository.DidNotReceive()
            .CreateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryCreateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();

        SetupNoDuplicateEffectiveFrom();
        _laborLawRuleRepository.CreateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
