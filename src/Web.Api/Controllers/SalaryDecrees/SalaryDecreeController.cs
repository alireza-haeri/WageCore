using Microsoft.AspNetCore.Authorization;
using Shared.Web.DateTimeHandling;
using Core.Contracts.Employees;

namespace Web.Api.Controllers.SalaryDecrees;

[Authorize]
[Tags("SalaryDecree")]
[Route("api/v1/salary-decrees")]
public class SalaryDecreeController(IMediator mediator) : BaseController
{
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateSalaryDecree")]
    public async Task<ActionResult<Result<CreateSalaryDecreeCommandResponse>>> CreateSalaryDecree(
        [FromBody] CreateSalaryDecreeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSalaryDecreeCommand(
            UserId: UserId,
            EmployeeId: request.EmployeeId,
            SalaryProfile: new SalaryDecreeDto(
                request.SalaryProfile.EffectiveFrom.ToDateOnly(),
                request.SalaryProfile.BaseDailySalary,
                request.SalaryProfile.AttractionAllowance,
                request.SalaryProfile.SupervisionAllowance,
                request.SalaryProfile.ShiftType,
                request.SalaryProfile.ContractType,
                request.SalaryProfile.TransportationAllowanceNet,
                request.SalaryProfile.MaritalStatus,
                request.SalaryProfile.ChildrenCount,
                request.SalaryProfile.IsTaxSubject,
                new EmployeeInsuranceDto(
                    request.SalaryProfile.Insurance.InsuranceNumber,
                    request.SalaryProfile.Insurance.PositionInInsuranceList,
                    request.SalaryProfile.Insurance.IsSubjectTo7PercentInsurance,
                    request.SalaryProfile.Insurance.IsSubjectTo20PercentInsurance,
                    request.SalaryProfile.Insurance.IsSubjectTo3PercentInsurance,
                    request.SalaryProfile.Insurance.IsSubjectTo4PercentInsurance,
                    request.SalaryProfile.Insurance.InsuranceCalculationProfile))
        ), cancellationToken);

        return Result(result);
    }

    [HttpPut("{salaryDecreeId:guid}")]
    [SwaggerOperation(OperationId = "UpdateSalaryDecree")]
    public async Task<ActionResult<Result<bool>>> UpdateSalaryDecree(
        [FromBody] UpdateSalaryDecreeRequest request,
        Guid salaryDecreeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSalaryDecreeCommand(
            UserId: UserId,
            EmployeeId: request.EmployeeId,
            SalaryDecreeId: salaryDecreeId,
            SalaryProfile: new SalaryDecreeDto(
                request.SalaryProfile.EffectiveFrom.ToDateOnly(),
                request.SalaryProfile.BaseDailySalary,
                request.SalaryProfile.AttractionAllowance,
                request.SalaryProfile.SupervisionAllowance,
                request.SalaryProfile.ShiftType,
                request.SalaryProfile.ContractType,
                request.SalaryProfile.TransportationAllowanceNet,
                request.SalaryProfile.MaritalStatus,
                request.SalaryProfile.ChildrenCount,
                request.SalaryProfile.IsTaxSubject,
                new EmployeeInsuranceDto(
                    request.SalaryProfile.Insurance.InsuranceNumber,
                    request.SalaryProfile.Insurance.PositionInInsuranceList,
                    request.SalaryProfile.Insurance.IsSubjectTo7PercentInsurance,
                    request.SalaryProfile.Insurance.IsSubjectTo20PercentInsurance,
                    request.SalaryProfile.Insurance.IsSubjectTo3PercentInsurance,
                    request.SalaryProfile.Insurance.IsSubjectTo4PercentInsurance,
                    request.SalaryProfile.Insurance.InsuranceCalculationProfile))
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{salaryDecreeId:guid}")]
    [SwaggerOperation(OperationId = "DeleteSalaryDecree")]
    public async Task<ActionResult<Result<bool>>> DeleteSalaryDecree(
        [FromQuery] Guid employeeId,
        Guid salaryDecreeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSalaryDecreeCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            SalaryDecreeId: salaryDecreeId
        ), cancellationToken);

        return Result(result);
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetSalaryDecrees")]
    public async Task<ActionResult<Result<PagedResult<GetSalaryDecreesResponse>>>> GetSalaryDecrees(
        [FromQuery] GetSalaryDecreesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSalaryDecreesQuery(
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
                .Map(sp => new GetSalaryDecreesResponse(
                    sp.SalaryDecreeId,
                    sp.EmployeeId,
                    sp.EmployeeName,
                    sp.PersonalCode,
                    sp.WorkshopName,
                    sp.DepartmentName,
                    PersianDate.FromDateOnly(sp.EffectiveFrom).ToDisplay(UserPersianDateFormat),
                    sp.BaseDailySalary,
                    sp.Status)
                )
            );

        return Result(response);
    }

    [HttpGet("{salaryDecreeId:guid}/edit")]
    [SwaggerOperation(OperationId = "GetSalaryDecreeForEdit")]
    public async Task<ActionResult<Result<GetSalaryDecreeForEditResponse>>> GetSalaryDecreeForEdit(
        Guid salaryDecreeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSalaryDecreeForEditQuery(
            UserId: UserId,
            SalaryDecreeId: salaryDecreeId
        ), cancellationToken);

        var response = result.Map(sp => new GetSalaryDecreeForEditResponse(
            sp.SalaryDecreeId,
            sp.EmployeeId,
            PersianDate.ToRawValue(sp.EffectiveFrom),
            sp.BaseDailySalary,
            sp.AttractionAllowance,
            sp.SupervisionAllowance,
            sp.ShiftType,
            sp.ContractType,
            sp.TransportationAllowanceNet,
            sp.MaritalStatus,
            sp.ChildrenCount,
            sp.IsTaxSubject,
            sp.InsuranceNumber,
            sp.PositionInInsuranceList,
            sp.IsSubjectTo7PercentInsurance,
            sp.IsSubjectTo20PercentInsurance,
            sp.IsSubjectTo3PercentInsurance,
            sp.IsSubjectTo4PercentInsurance,
            sp.InsuranceCalculationProfile
        ));

        return Result(response);
    }
}
