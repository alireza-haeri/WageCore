namespace Application.Tests.Features.Users.Commands.LoginUser;

public class LoginUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly LoginUserCommandHandler _handler;
    private readonly UserBuilder _userBuilder;

    private const string ValidPhoneNumber = "09123456789";
    private const string ValidEmail = "ali@gmail.com";
    private const string ValidPassword = "123456";

    public LoginUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _userBuilder = new UserBuilder();

        _handler = new LoginUserCommandHandler(
            _userRepository,
            _jwtTokenService
        );
    }

    private void SetupSuccessfulLogin(User user, IReadOnlyCollection<string>? roles = null)
    {
        _userRepository.CheckPasswordAsync(user, ValidPassword, Arg.Any<CancellationToken>())
            .Returns(true);

        _userRepository.GetRolesAsync(user.Id, Arg.Any<CancellationToken>())
            .Returns(roles ?? []);

        _jwtTokenService.GenerateToken(user, Arg.Any<IReadOnlyCollection<string>?>())
            .Returns(new JwtTokenResponse("fake-token", 60));
    }

    [Fact]
    public async Task Handle_WithValidPhoneAndPassword_ShouldReturnToken()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);
        var user = _userBuilder
            .WithPhoneNumber(ValidPhoneNumber)
            .CreateResult()
            .ShouldBeSuccess();

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns(user);

        SetupSuccessfulLogin(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Token.Should().Be("fake-token");
        response.ExpireInMinutes.Should().Be(60);
    }

    [Fact]
    public async Task Handle_WithValidEmailAndPassword_ShouldReturnToken()
    {
        var command = new LoginUserCommand(null, ValidEmail, ValidPassword);
        var user = _userBuilder
            .WithEmail(ValidEmail)
            .CreateResult()
            .ShouldBeSuccess();

        _userRepository.GetAsync(null, ValidEmail, Arg.Any<CancellationToken>())
            .Returns(user);

        SetupSuccessfulLogin(user);

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Token.Should().Be("fake-token");
        response.ExpireInMinutes.Should().Be(60);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnValidationFailure()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        var failure = result.ShouldBeFailure();
        failure.Should().ContainKey("PhoneNumber");
        failure.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldReturnValidationFailure()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);
        var user = _userBuilder
            .WithPhoneNumber(ValidPhoneNumber)
            .CreateResult()
            .ShouldBeSuccess();

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository.CheckPasswordAsync(user, ValidPassword, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        var failure = result.ShouldBeFailure();
        failure.Should().ContainKey("PhoneNumber");
        failure.Should().ContainKey("Email");
    }

    [Fact]
    public async Task Handle_ShouldCallGetAsyncOnce()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);
        var user = _userBuilder
            .WithPhoneNumber(ValidPhoneNumber)
            .CreateResult()
            .ShouldBeSuccess();

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns(user);

        SetupSuccessfulLogin(user);

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.Received(1).GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallCheckPasswordAsyncOnceWhenUserExists()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);
        var user = _userBuilder
            .WithPhoneNumber(ValidPhoneNumber)
            .CreateResult()
            .ShouldBeSuccess();

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns(user);

        SetupSuccessfulLogin(user);

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.Received(1).CheckPasswordAsync(user, ValidPassword, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotCallCheckPasswordAsync()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        await _handler.Handle(command, CancellationToken.None);

        await _userRepository.DidNotReceive().CheckPasswordAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPasswordIsWrong_ShouldNotCallGenerateToken()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);
        var user = _userBuilder
            .WithPhoneNumber(ValidPhoneNumber)
            .CreateResult()
            .ShouldBeSuccess();

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns(user);

        _userRepository.CheckPasswordAsync(user, ValidPassword, Arg.Any<CancellationToken>())
            .Returns(false);

        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyCollection<string>?>());
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldNotCallGenerateToken()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<User>(), Arg.Any<IReadOnlyCollection<string>?>());
    }

    [Fact]
    public async Task Handle_ShouldCallGenerateTokenOnceWhenSuccess()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);
        var user = _userBuilder
            .WithPhoneNumber(ValidPhoneNumber)
            .CreateResult()
            .ShouldBeSuccess();

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns(user);

        SetupSuccessfulLogin(user);

        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.Received(1).GenerateToken(user, Arg.Any<IReadOnlyCollection<string>?>());
    }

    [Fact]
    public async Task Handle_ShouldPassUserRolesToGenerateToken()
    {
        var command = new LoginUserCommand(ValidPhoneNumber, null, ValidPassword);
        var user = _userBuilder
            .WithPhoneNumber(ValidPhoneNumber)
            .CreateResult()
            .ShouldBeSuccess();

        _userRepository.GetAsync(ValidPhoneNumber, null, Arg.Any<CancellationToken>())
            .Returns(user);

        SetupSuccessfulLogin(user, [ApplicationRoles.SiteManagerRule]);

        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.Received(1).GenerateToken(
            user,
            Arg.Is<IReadOnlyCollection<string>>(roles => roles.Contains(ApplicationRoles.SiteManagerRule)));
    }
}
