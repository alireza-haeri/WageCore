using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers.Departments;

[Authorize]
[Tags("Department")]
[Route("api/v1/departments")]
public class DepartmentController(IMediator mediator) : BaseController
{
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateDepartment")]
    public async Task<ActionResult<Result<CreateDepartmentCommandResponse>>> CreateDepartment(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateDepartmentCommand(
            UserId: UserId,
            WorkshopId: request.WorkshopId,
            Name: request.Name
        ), cancellationToken);

        return Result(result);
    }

    [HttpPut("{departmentId:guid}")]
    [SwaggerOperation(OperationId = "UpdateDepartment")]
    public async Task<ActionResult<Result<bool>>> UpdateDepartment(
        [FromBody] UpdateDepartmentRequest request,
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateDepartmentCommand(
            UserId: UserId,
            DepartmentId: departmentId,
            Name: request.Name
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{departmentId:guid}")]
    [SwaggerOperation(OperationId = "DeleteDepartment")]
    public async Task<ActionResult<Result<bool>>> DeleteDepartment(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteDepartmentCommand(
            UserId: UserId,
            DepartmentId: departmentId
        ), cancellationToken);

        return Result(result);
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetUserDepartments")]
    public async Task<ActionResult<Result<PagedResult<GetUserDepartmentsQueryResponse>>>> GetUserDepartments(
        [FromQuery] GetUserDepartmentsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserDepartmentsQuery(
            UserId: UserId,
            Pagination: request.Pagination,
            SearchName: request.SearchName,
            WorkshopId: request.WorkshopId
        ), cancellationToken);

        return Result(result);
    }

    [HttpGet("names")]
    [SwaggerOperation(OperationId = "GetUserDepartmentsName")]
    public async Task<ActionResult<Result<GetUserDepartmentsNameQueryResponse>>> GetUserDepartmentsName(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserDepartmentsNameQuery(
            UserId: UserId
        ), cancellationToken);

        return Result(result);
    }

    [HttpGet("{departmentId:guid}/edit")]
    [SwaggerOperation(OperationId = "GetDepartmentForEdit")]
    public async Task<ActionResult<Result<GetDepartmentForEditQueryResponse>>> GetDepartmentForEdit(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDepartmentForEditQuery(
            UserId: UserId,
            DepartmentId: departmentId
        ), cancellationToken);

        return Result(result);
    }
}
