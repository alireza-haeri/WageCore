namespace Application.Tests.Features.LaborLawRules.Command.UpdateLaborLawRule;

public class UpdateLaborLawRuleCommandHandlerTests
{
    private readonly ILaborLawRuleRepository _laborLawRuleRepository;
    private readonly ILaborLawRuleQuery _laborLawRuleQuery;
    private readonly UpdateLaborLawRuleCommandHandler _handler;
    private readonly LaborLawRuleItemBuilder _builder;

    private static readonly Guid ValidRuleId = Guid.NewGuid();
    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const decimal ValidValue = 103_909_680m;

    public UpdateLaborLawRuleCommandHandlerTests()
    {
        _laborLawRuleRepository = Substitute.For<ILaborLawRuleRepository>();
        _laborLawRuleQuery = Substitute.For<ILaborLawRuleQuery>();
        _handler = new UpdateLaborLawRuleCommandHandler(_laborLawRuleRepository, _laborLawRuleQuery);
        _builder = new LaborLawRuleItemBuilder();
    }

    private LaborLawRuleItem CreateValidRule()
    {
        return _builder
            .WithId(ValidRuleId)
            .CreateResult()
            .ShouldBeSuccess();
    }

    private static UpdateLaborLawRuleCommand CreateValidCommand(
        Guid? ruleId = null,
        LaborLawRuleKey? key = LaborLawRuleKey.MinimumDailySalary,
        decimal? value = ValidValue,
        DateOnly? effectiveFrom = null) =>
        new(ruleId ?? ValidRuleId, key, value, effectiveFrom ?? ValidEffectiveFrom);

    private void SetupNoDuplicateEffectiveFrom()
    {
        _laborLawRuleQuery.IsExistEffectiveFrom(
                Arg.Any<LaborLawRuleKey>(),
                Arg.Any<DateOnly>(),
                Arg.Any<Guid?>(),
                Arg.Any<CancellationToken>())
            .Returns(false);
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldUpdateRuleAndReturnTrue()
    {
        var command = CreateValidCommand();
        var rule = CreateValidRule();

        _laborLawRuleRepository.GetByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(rule);
        SetupNoDuplicateEffectiveFrom();
        _laborLawRuleRepository.UpdateAsync(rule, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        using (new AssertionScope())
        {
            rule.Value.Should().Be(ValidValue);
            rule.EffectiveFrom.Should().Be(ValidEffectiveFrom);
        }
    }

    [Fact]
    public async Task Handle_WhenRuleNotFound_ShouldReturnNotFoundFailure()
    {
        var command = CreateValidCommand();

        _laborLawRuleRepository.GetByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns((LaborLawRuleItem?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
        await _laborLawRuleRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromAlreadyExists_ShouldReturnValidationFailure()
    {
        var command = CreateValidCommand();
        var rule = CreateValidRule();

        _laborLawRuleRepository.GetByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(rule);
        _laborLawRuleQuery.IsExistEffectiveFrom(
                LaborLawRuleKey.MinimumDailySalary,
                ValidEffectiveFrom,
                ValidRuleId,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("تاریخ اجرا تکراری است.", BadResultType.Validation);
        await _laborLawRuleRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(value: -1);
        var rule = CreateValidRule();

        _laborLawRuleRepository.GetByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(rule);
        SetupNoDuplicateEffectiveFrom();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("مقدار قانون نمیتواند منفی باشد.", BadResultType.General);
        await _laborLawRuleRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryUpdateFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand();
        var rule = CreateValidRule();

        _laborLawRuleRepository.GetByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(rule);
        SetupNoDuplicateEffectiveFrom();
        _laborLawRuleRepository.UpdateAsync(rule, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
