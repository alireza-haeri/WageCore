using Microsoft.AspNetCore.Authorization;
using Shared.Web.DateTimeHandling;
using Core.Contracts.Employees;

namespace Web.Api.Controllers.EmployeeSalaryProfiles;

[Authorize]
[Tags("EmployeeSalaryProfile")]
[Route("api/v1/employee-salary-profiles")]
public class EmployeeSalaryProfileController(IMediator mediator) : BaseController
{
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateEmployeeSalaryProfile")]
    public async Task<ActionResult<Result<CreateEmployeeSalaryProfileCommandResponse>>> CreateEmployeeSalaryProfile(
        [FromBody] CreateEmployeeSalaryProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateEmployeeSalaryProfileCommand(
            UserId: UserId,
            EmployeeId: request.EmployeeId,
            SalaryProfile: new EmployeeSalaryProfileDto(
                request.SalaryProfile.EffectiveFrom.ToDateOnly(),
                request.SalaryProfile.BaseMonthlySalary,
                request.SalaryProfile.AttractionAllowance,
                request.SalaryProfile.SupervisionAllowance,
                request.SalaryProfile.SeniorityBaseApplicationMode,
                request.SalaryProfile.SeniorityBaseCalculationMethod,
                request.SalaryProfile.YearEndSeniorityMode,
                request.SalaryProfile.ShiftType,
                request.SalaryProfile.HousingAllowance,
                request.SalaryProfile.FoodAllowance,
                request.SalaryProfile.ChildAllowancePerChild,
                request.SalaryProfile.TransportationAllowanceNet,
                request.SalaryProfile.KaranehAmountNet)
        ), cancellationToken);

        return Result(result);
    }

    [HttpPut("{employeeSalaryProfileId:guid}")]
    [SwaggerOperation(OperationId = "UpdateEmployeeSalaryProfile")]
    public async Task<ActionResult<Result<bool>>> UpdateEmployeeSalaryProfile(
        [FromBody] UpdateEmployeeSalaryProfileRequest request,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateEmployeeSalaryProfileCommand(
            UserId: UserId,
            EmployeeId: request.EmployeeId,
            EmployeeSalaryProfileId: employeeSalaryProfileId,
            SalaryProfile: new EmployeeSalaryProfileDto(
                request.SalaryProfile.EffectiveFrom.ToDateOnly(),
                request.SalaryProfile.BaseMonthlySalary,
                request.SalaryProfile.AttractionAllowance,
                request.SalaryProfile.SupervisionAllowance,
                request.SalaryProfile.SeniorityBaseApplicationMode,
                request.SalaryProfile.SeniorityBaseCalculationMethod,
                request.SalaryProfile.YearEndSeniorityMode,
                request.SalaryProfile.ShiftType,
                request.SalaryProfile.HousingAllowance,
                request.SalaryProfile.FoodAllowance,
                request.SalaryProfile.ChildAllowancePerChild,
                request.SalaryProfile.TransportationAllowanceNet,
                request.SalaryProfile.KaranehAmountNet)
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{employeeSalaryProfileId:guid}")]
    [SwaggerOperation(OperationId = "DeleteEmployeeSalaryProfile")]
    public async Task<ActionResult<Result<bool>>> DeleteEmployeeSalaryProfile(
        [FromQuery] Guid employeeId,
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteEmployeeSalaryProfileCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            EmployeeSalaryProfileId: employeeSalaryProfileId
        ), cancellationToken);

        return Result(result);
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetEmployeeSalaryProfiles")]
    public async Task<ActionResult<Result<PagedResult<GetEmployeeSalaryProfilesResponse>>>> GetEmployeeSalaryProfiles(
        [FromQuery] GetEmployeeSalaryProfilesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeSalaryProfilesQuery(
            UserId: UserId,
            Pagination: request.Pagination,
            EmployeeId: request.EmployeeId,
            Search: request.Search,
            Status: request.Status,
            WorkshopId: request.WorkshopId,
            DepartmentId: request.DepartmentId
        ), cancellationToken);

        var response = result
            .Map(paged => paged
                .Map(sp => new GetEmployeeSalaryProfilesResponse(
                    sp.EmployeeSalaryProfileId,
                    sp.EmployeeId,
                    sp.EmployeeName,
                    sp.PersonalCode,
                    sp.WorkshopName,
                    sp.DepartmentName,
                    PersianDate.FromDateOnly(sp.EffectiveFrom).ToDisplay(UserPersianDateFormat),
                    sp.BaseMonthlySalary,
                    sp.Status)
                )
            );

        return Result(response);
    }

    [HttpGet("{employeeSalaryProfileId:guid}/edit")]
    [SwaggerOperation(OperationId = "GetEmployeeSalaryProfileForEdit")]
    public async Task<ActionResult<Result<GetEmployeeSalaryProfileForEditResponse>>> GetEmployeeSalaryProfileForEdit(
        Guid employeeSalaryProfileId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeeSalaryProfileForEditQuery(
            UserId: UserId,
            EmployeeSalaryProfileId: employeeSalaryProfileId
        ), cancellationToken);

        var response = result.Map(sp => new GetEmployeeSalaryProfileForEditResponse(
            sp.EmployeeSalaryProfileId,
            sp.EmployeeId,
            PersianDate.ToRawValue(sp.EffectiveFrom),
            sp.BaseMonthlySalary,
            sp.AttractionAllowance,
            sp.SupervisionAllowance,
            sp.SeniorityBaseApplicationMode,
            sp.SeniorityBaseCalculationMethod,
            sp.YearEndSeniorityMode,
            sp.ShiftType,
            sp.HousingAllowance,
            sp.FoodAllowance,
            sp.ChildAllowancePerChild,
            sp.TransportationAllowanceNet,
            sp.KaranehAmountNet
        ));

        return Result(response);
    }
}
