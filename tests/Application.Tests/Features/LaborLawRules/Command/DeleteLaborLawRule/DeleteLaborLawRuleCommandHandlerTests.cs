namespace Application.Tests.Features.LaborLawRules.Command.DeleteLaborLawRule;

public class DeleteLaborLawRuleCommandHandlerTests
{
    private readonly ILaborLawRuleRepository _laborLawRuleRepository;
    private readonly DeleteLaborLawRuleCommandHandler _handler;
    private readonly LaborLawRuleItemBuilder _builder;

    private static readonly Guid ValidRuleId = Guid.NewGuid();

    public DeleteLaborLawRuleCommandHandlerTests()
    {
        _laborLawRuleRepository = Substitute.For<ILaborLawRuleRepository>();
        _handler = new DeleteLaborLawRuleCommandHandler(_laborLawRuleRepository);
        _builder = new LaborLawRuleItemBuilder();
    }

    private LaborLawRuleItem CreateValidRule()
    {
        return _builder
            .WithId(ValidRuleId)
            .CreateResult()
            .ShouldBeSuccess();
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldDeleteRuleAndReturnTrue()
    {
        var command = new DeleteLaborLawRuleCommand(ValidRuleId);
        var rule = CreateValidRule();

        _laborLawRuleRepository.GetByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(rule);
        _laborLawRuleRepository.DeleteAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Should().BeTrue();
        await _laborLawRuleRepository.Received(1).DeleteAsync(ValidRuleId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRuleNotFound_ShouldReturnNotFoundFailure()
    {
        var command = new DeleteLaborLawRuleCommand(ValidRuleId);

        _laborLawRuleRepository.GetByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns((LaborLawRuleItem?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.NotFound);
        await _laborLawRuleRepository.DidNotReceive()
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRepositoryDeleteFails_ShouldReturnGeneralFailure()
    {
        var command = new DeleteLaborLawRuleCommand(ValidRuleId);
        var rule = CreateValidRule();

        _laborLawRuleRepository.GetByIdAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(rule);
        _laborLawRuleRepository.DeleteAsync(ValidRuleId, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }
}
