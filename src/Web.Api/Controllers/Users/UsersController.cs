namespace Web.Api.Controllers.Users;

[Tags("Identity")]
[Route("api/v1/users")]
public class UsersController(IMediator mediator) : BaseController
{
    [HttpPost("register")]
    [SwaggerOperation(OperationId = "RegisterUser")]
    public async Task<ActionResult<Result<RegisterUserCommandResponse>>> RegisterOrLogin(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new RegisterUserCommand(request.PhoneNumber, request.Email, request.FullName, request.Password)
            , cancellationToken);
        return Result(result);
    }
    
    
    [HttpPost("login")]
    [SwaggerOperation(OperationId = "LoginUser")]
    public async Task<ActionResult<Result<LoginUserCommandResponse>>> Login(
        [FromBody] LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new LoginUserCommand(request.PhoneNumber, request.Email, request.Password)
            , cancellationToken);
        return Result(result);
    }
}