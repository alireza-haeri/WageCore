namespace Application.Tests.Features.LaborLawRules.Command.CreateLaborLawRule;

public class CreateLaborLawRuleCommandHandlerTests
{
    private readonly ILaborLawRuleRepository _laborLawRuleRepository;
    private readonly CreateLaborLawRuleCommandHandler _handler;

    private static readonly DateOnly ValidEffectiveFrom = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
    private const decimal ValidValue = 71_661_840m;

    public CreateLaborLawRuleCommandHandlerTests()
    {
        _laborLawRuleRepository = Substitute.For<ILaborLawRuleRepository>();
        _handler = new CreateLaborLawRuleCommandHandler(_laborLawRuleRepository);
    }

    private static CreateLaborLawRuleCommand CreateValidCommand(
        LaborLawRuleKey? key = LaborLawRuleKey.MinimumMonthlySalary,
        decimal? value = ValidValue,
        DateOnly? effectiveFrom = null) =>
        new(key, value, effectiveFrom ?? ValidEffectiveFrom);

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateRuleAndReturnId()
    {
        var command = CreateValidCommand();
        var createdId = Guid.NewGuid();

        _laborLawRuleRepository.CreateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>())
            .Returns(createdId);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.LaborLawRuleId.Should().Be(createdId);

        await _laborLawRuleRepository.Received(1).CreateAsync(
            Arg.Is<LaborLawRuleItem>(x =>
                x.Key == LaborLawRuleKey.MinimumMonthlySalary &&
                x.Value == ValidValue &&
                x.EffectiveFrom == ValidEffectiveFrom),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnGeneralFailure()
    {
        var command = CreateValidCommand(value: -1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure("مقدار قانون نمیتواند منفی باشد.", BadResultType.General);
        await _laborLawRuleRepository.DidNotReceive()
            .CreateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEffectiveFromIsNull_ShouldReturnGeneralFailure()
    {
        var command = new CreateLaborLawRuleCommand(
            LaborLawRuleKey.MinimumMonthlySalary,
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

        _laborLawRuleRepository.CreateAsync(Arg.Any<LaborLawRuleItem>(), Arg.Any<CancellationToken>())
            .Returns((Guid?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
