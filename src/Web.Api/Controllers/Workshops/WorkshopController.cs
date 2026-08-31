using Microsoft.AspNetCore.Authorization;
using Shared.Web.DateTimeHandling;

namespace Web.Api.Controllers.Workshops;

[Authorize]
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
            RegistrationDate: request.RegistrationDate.ToDateOnly(),
            NationalId: request.NationalId,
            SocialSecurityNumber: request.SocialSecurityNumber,
            PostalCode: request.PostalCode,
            EconomicCode: request.EconomicCode
        ), cancellationToken);

        return Result(result);
    }

    [HttpPut("{workshopId:guid}")]
    [SwaggerOperation(OperationId = "UpdateWorkshop")]
    public async Task<ActionResult<Result<bool>>> UpdateWorkshop(
        [FromBody] UpdateWorkshopRequest request,
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateWorkshopCommand(
            UserId: UserId,
            WorkshopId: workshopId,
            Name: request.Name,
            Address: request.Address,
            RegistrationDate: request.RegistrationDate.ToDateOnly(),
            NationalId: request.NationalId,
            SocialSecurityNumber: request.SocialSecurityNumber,
            PostalCode: request.PostalCode,
            EconomicCode: request.EconomicCode
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{workshopId:guid}")]
    [SwaggerOperation(OperationId = "DeleteWorkshop")]
    public async Task<ActionResult<Result<bool>>> DeleteWorkshop(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteWorkshopCommand(
            UserId: UserId,
            WorkshopId: workshopId
        ), cancellationToken);

        return Result(result);
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetUserWorkshops")]
    public async Task<ActionResult<Result<PagedResult<GetUserWorkshopsResponse>>>> GetUserWorkshops(
        [FromQuery] GetUserWorkshopsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserWorkshopsQuery(
            UserId: UserId,
            Pagination: request.Pagination,
            SearchName: request.SearchName
        ), cancellationToken);

        var response = result
            .Map(paged => paged
                .Map(w =>
                    new GetUserWorkshopsResponse(
                        w.Id,
                        w.Name,
                        w.Address,
                        w.NationalId,
                        PersianDate.FromDateOnly(w.RegistrationDate).ToDisplay(UserPersianDateFormat),
                        w.EmployeesCount,
                        w.DepartmentsCount,
                        w.SocialSecurityNumber,
                        w.EconomicCode
                    )
                )
            );

        return Result(response);
    }

    [HttpGet("names")]
    [SwaggerOperation(OperationId = "GetUserWorkshopsName")]
    public async Task<ActionResult<Result<GetUserWorkshopsNameQueryResponse>>> GetUserWorkshopsName(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserWorkshopsNameQuery(
            UserId: UserId
        ), cancellationToken);

        return Result(result);
    }

    [HttpGet("{workshopId:guid}/edit")]
    [SwaggerOperation(OperationId = "GetWorkshopForEdit")]
    public async Task<ActionResult<Result<GetWorkshopForEditResponse>>> GetWorkshopForEdit(
        Guid workshopId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWorkshopForEditQuery(
            UserId: UserId,
            WorkshopId: workshopId
        ), cancellationToken);

        var response = result.Map(w=> new GetWorkshopForEditResponse(
            w.Name,
            w.Address,
            PersianDate.ToRawValue(w.RegistrationDate),
            w.NationalId,
            w.PostalCode,
            w.SocialSecurityNumber,
            w.EconomicCode
        ));
        
        return Result(response);
    }
}