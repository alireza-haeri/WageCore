namespace Application.Tests.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RegisterUserCommandHandler _handler;
    private readonly UserBuilder _userBuilder;

    private const string ValidPhoneNumber = "09123456789";
    private const string ValidEmail = "ali@gmail.com";
    private const string ValidPassword = "123456";
    private const string ValidFullName = "علی رضایی";

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _userBuilder = new UserBuilder();

        _handler = new RegisterUserCommandHandler(
            _userRepository,
            _jwtTokenService
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateUserAndReturnToken()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(true, new Dictionary<string, string[]>()));

        _jwtTokenService.GenerateToken(Arg.Any<User>())
            .Returns(new JwtTokenResponse("fake-token", 60));

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Token.Should().Be("fake-token");
        response.ExpireInMinutes.Should().Be(60);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldReturnValidationFailure()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        var failure = result.ShouldBeFailure();
        failure.Should().ContainKey("PhoneNumber");
        failure.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Handle_WhenDomainCreationFails_ShouldReturnGeneralFailure()
    {
        var invalidPhone = "123";
        var command = new RegisterUserCommand(invalidPhone, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(invalidPhone, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_WhenCreateFails_ShouldReturnGeneralFailure()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(false, new Dictionary<string, string[]>()));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.ShouldBeFailure(null, BadResultType.General);
    }

    [Fact]
    public async Task Handle_ShouldCallExistsAsyncOnce()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(true, new Dictionary<string, string[]>()));

        _jwtTokenService.GenerateToken(Arg.Any<User>())
            .Returns(new JwtTokenResponse("fake-token", 60));

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.Received(1).ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallCreateAsyncOnce()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(true, new Dictionary<string, string[]>()));

        _jwtTokenService.GenerateToken(Arg.Any<User>())
            .Returns(new JwtTokenResponse("fake-token", 60));

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.Received(1).CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistsFails_ShouldNotCallCreateAsync()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCreateFails_ShouldNotCallGenerateToken()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(false, new Dictionary<string, string[]>()));

        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_WhenDomainFails_ShouldNotCallCreateAsync()
    {
        var invalidPhone = "123";
        var command = new RegisterUserCommand(invalidPhone, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(invalidPhone, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGenerateTokenWithCreatedUser()
    {
        var command = new RegisterUserCommand(ValidPhoneNumber, ValidEmail, ValidFullName, ValidPassword);

        _userRepository.ExistsAsync(ValidPhoneNumber, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(false);

        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(true, new Dictionary<string, string[]>()));

        _jwtTokenService.GenerateToken(Arg.Any<User>())
            .Returns(new JwtTokenResponse("fake-token", 60));

        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.Received(1).GenerateToken(Arg.Any<User>());
    }
}