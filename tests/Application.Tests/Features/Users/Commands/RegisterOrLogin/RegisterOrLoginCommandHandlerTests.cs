namespace Application.Tests.Features.Users.Commands.RegisterOrLogin;

public class RegisterOrLoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly RegisterOrLoginCommandHandler _handler;
    private readonly UserBuilder  _userBuilder;

    private const string ValidPhone = "09123456789";
    private const string ValidPassword = "123456";

    public RegisterOrLoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _userBuilder  = new UserBuilder();
        
        _handler = new RegisterOrLoginCommandHandler(
            _userRepository,
            _jwtTokenService
        );
    }

    [Fact]
    public async Task Handle_NewUser_WithValidData_ShouldRegisterAndReturnToken()
    {
        var command = new RegisterOrLoginCommand(ValidPhone, ValidPassword);

        _userRepository.GetAsync(ValidPhone, Arg.Any<CancellationToken>())
            .Returns((User?)null);

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
    public async Task Handle_ExistingUser_WithCorrectPassword_ShouldReturnToken()
    {
        var command = new RegisterOrLoginCommand(ValidPhone, ValidPassword);
        var existingUser = _userBuilder.CreateResult().Response;

        _userRepository.GetAsync(ValidPhone, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        _userRepository.CheckPasswordAsync(ValidPhone, ValidPassword, Arg.Any<CancellationToken>())
            .Returns(true);

        _jwtTokenService.GenerateToken(existingUser!)
            .Returns(new JwtTokenResponse("login-token", 60));

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeSuccess();
        response.Token.Should().Be("login-token");
    }
    
    [Fact]
    public async Task Handle_ExistingUser_WithWrongPassword_ShouldReturnNotFound()
    {
        var command = new RegisterOrLoginCommand(ValidPhone, ValidPassword);
        var existingUser = _userBuilder.CreateResult().Response;

        _userRepository.GetAsync(ValidPhone, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        _userRepository.CheckPasswordAsync(ValidPhone, ValidPassword, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailure(null, BadResultType.NotFound);
    }

    [Fact]
    public async Task Handle_NewUser_WithInvalidPhoneNumber_ShouldReturnGeneralFailure()
    {
        var inValidPhone = "123";
        var command = new RegisterOrLoginCommand(inValidPhone, ValidPassword);

        _userRepository.GetAsync(inValidPhone, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailure(null, BadResultType.General);
    }
    
    [Fact]
    public async Task Handle_NewUser_WhenCreateFails_ShouldReturnValidationFailure()
    {
        var command = new RegisterOrLoginCommand(ValidPhone, ValidPassword);

        _userRepository.GetAsync(ValidPhone, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var errors = new Dictionary<string, string[]>
        {
            { "Password", ["رمز عبور ضعیف است"] }
        };

        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(false, errors));

        var result = await _handler.Handle(command, CancellationToken.None);

        var response = result.ShouldBeFailure();
        response.Should().ContainKey("Password");
    }

    [Fact]
    public async Task Handle_NewUser_ShouldCallCreateAsyncOnce()
    {
        var command = new RegisterOrLoginCommand(ValidPhone, ValidPassword);
        
        _userRepository.GetAsync(ValidPhone, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        
        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(true, new Dictionary<string, string[]>()));
        
        _jwtTokenService.GenerateToken(Arg.Any<User>())
            .Returns(new JwtTokenResponse("fake-token", 60));
        
        await _handler.Handle(command, CancellationToken.None);
        
        await _userRepository.Received(1).CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingUser_ShouldNotCallCreateAsync()
    {
        var command = new RegisterOrLoginCommand(ValidPhone, ValidPassword);
        
        _userRepository.GetAsync(ValidPhone, Arg.Any<CancellationToken>())
            .Returns(_userBuilder.CreateResult().Response);
        
        _userRepository.CheckPasswordAsync(ValidPhone, ValidPassword, Arg.Any<CancellationToken>())
            .Returns(true);
        
        _jwtTokenService.GenerateToken(Arg.Any<User>())
            .Returns(new JwtTokenResponse("fake-token", 60));
        
        await _handler.Handle(command, CancellationToken.None);
        
        await _userRepository.DidNotReceive().CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NewUserWithWrongPassword_ShouldNotCallGenerateTokenAsync()
    {
        var command = new RegisterOrLoginCommand(ValidPhone, ValidPassword);
        
        _userRepository.GetAsync(ValidPhone, Arg.Any<CancellationToken>())
            .Returns(_userBuilder.CreateResult().Response);
        
        _userRepository.CheckPasswordAsync(ValidPhone, ValidPassword, Arg.Any<CancellationToken>())
            .Returns(false);
        
        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task Handle_NewUserFailure_ShouldNotCallGenerateTokenAsync()
    {
        var command = new RegisterOrLoginCommand(ValidPhone, ValidPassword);
        
        _userRepository.GetAsync(ValidPhone, Arg.Any<CancellationToken>())
            .Returns((User?)null);
        
        _userRepository.CreateAsync(Arg.Any<User>(), ValidPassword, Arg.Any<CancellationToken>())
            .Returns(new IdentityResult(false,new Dictionary<string, string[]>()));
        
        await _handler.Handle(command, CancellationToken.None);

        _jwtTokenService.DidNotReceive().GenerateToken(Arg.Any<User>());
    }
}