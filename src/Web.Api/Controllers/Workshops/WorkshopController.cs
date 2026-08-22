namespace Web.Api.Controllers.Workshops;

[Tags("Workshop")]
[Route("api/v1/workshops")]
public class WorkshopController(IMediator mediator) : BaseController
{
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateWorkshop")]
    public async Task<ActionResult<Result<CreateWorkshopCommandResponse>>> CreateWorkshop(
        [FromBody] CreateWorkshopRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateWorkshopCommand(
            UserId: UserId,
            Name: request.Name,
            Address: request.Address,
            Region: request.Region,
            RegistrationDate: request.RegistrationDate.ToDateOnly(),
            NationalId: request.NationalId,
            PostalCode: request.PostalCode
            ), CancellationToken.None);
        
        return Result(result);
    }
}