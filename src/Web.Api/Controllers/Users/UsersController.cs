namespace Web.Api.Controllers.Users;

[Tags("Identity")]
[Route("api/v1/users")]
public class IdentityController(IMediator mediator) : BaseController
{
    [HttpPost("authentication")]
    [SwaggerOperation(OperationId = "RegisterOrLogin")]
    public async Task<ActionResult<Result<RegisterOrLoginCommandResponse>>> RegisterOrLogin(
        [FromBody] RegisterOrLoginRequest request, CancellationToken cancellationToken)
    {
        var result =
            await mediator.Send(new RegisterOrLoginCommand(request.PhoneNumber, request.Password), cancellationToken);
        return Result(result);
    }
}