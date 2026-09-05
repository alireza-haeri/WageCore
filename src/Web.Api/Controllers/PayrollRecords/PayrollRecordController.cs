using Application.Features.PayrollRecords.Query.GetPayrollRecords;
using Microsoft.AspNetCore.Authorization;

namespace Web.Api.Controllers.PayrollRecords;

[Authorize]
[Tags("PayrollRecord")]
[Route("api/v1/payroll-records")]
public class PayrollRecordController(IMediator mediator) : BaseController
{
    [HttpGet]
    [SwaggerOperation(OperationId = "GetPayrollRecords")]
    public async Task<ActionResult<Result<PagedResult<GetPayrollRecordsResponse>>>> GetPayrollRecords(
        [FromQuery] GetPayrollRecordsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPayrollRecordsQuery(
            UserId: UserId,
            Pagination: request.Pagination,
            Search: request.Search,
            WorkshopId: request.WorkshopId,
            DepartmentId: request.DepartmentId,
            PersianYear: request.PersianYear,
            PersianMonth: request.PersianMonth
        ), cancellationToken);

        var response = result
            .Map(paged => paged
                .Map(pr => new GetPayrollRecordsResponse(
                    pr.PayrollRecordId,
                    pr.EmployeeId,
                    pr.EmployeeName,
                    pr.PersonalCode,
                    pr.WorkshopName,
                    pr.DepartmentName,
                    PersianDate.FromDateOnly(pr.PeriodEnd).ToDisplay("MMMM yyyy"),
                    pr.WorkedDaysCount,
                    pr.OvertimeHours,
                    pr.GrossAmount,
                    pr.TotalDeductionsAmount,
                    pr.NetPayableAmount,
                    pr.Status)
                )
            );

        return Result(response);
    }

    [HttpGet("{payrollRecordId:guid}/edit")]
    [SwaggerOperation(OperationId = "GetPayrollRecordForEdit")]
    public async Task<ActionResult<Result<GetPayrollRecordForEditResponse>>> GetPayrollRecordForEdit(
        Guid payrollRecordId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPayrollRecordForEditQuery(
            UserId: UserId,
            PayrollRecordId: payrollRecordId
        ), cancellationToken);

        var response = result.Map(pr => new GetPayrollRecordForEditResponse(
            pr.PayrollRecordId,
            pr.EmployeeId,
            pr.EmployeeName,
            pr.PersonalCode,
            pr.PersianYear,
            pr.PersianMonth,
            pr.Work,
            pr.Status));

        return Result(response);
    }

    [HttpGet("{payrollRecordId:guid}/calculation-details")]
    [SwaggerOperation(OperationId = "GetPayrollRecordCalculationDetails")]
    public async Task<ActionResult<Result<GetPayrollRecordCalculationDetailsResponse>>> GetPayrollRecordCalculationDetails(
        Guid payrollRecordId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPayrollRecordCalculationDetailsQuery(
            UserId: UserId,
            PayrollRecordId: payrollRecordId
        ), cancellationToken);

        var response = result.Map(pr => new GetPayrollRecordCalculationDetailsResponse(
            pr.PayrollRecordId,
            pr.EmployeeId,
            pr.EmployeeName,
            pr.PersonalCode,
            pr.EmployeeHireDate,
            pr.Status,
            pr.PersianYear,
            pr.PersianMonth,
            pr.PeriodStart,
            pr.PeriodEnd,
            pr.PeriodDaysCount,
            pr.FridayCount,
            pr.DaysInYear,
            pr.StandardWorkingDaysCount,
            pr.WorkedDaysCount,
            pr.HolidaysCount,
            pr.LeaveHours,
            pr.OvertimeHours,
            pr.NightShiftHours,
            pr.FridayWorkHours,
            pr.HolidayWorkHours,
            pr.MissionDaysCount,
            pr.MissionHours,
            pr.MissionAmountOverride,
            pr.PerformanceBonusAmount,
            pr.CashBenefitsAmount,
            pr.AnnualBonusType,
            pr.IsEsfandPeriod,
            pr.MaxMonthlyOvertimeHours,
            pr.MaxFridayHours,
            pr.MaxNightShiftHours,
            pr.MaxMissionDaysCount,
            pr.MaxHolidayWorkHours,
            pr.DailyWorkingHours,
            pr.DecreeEffectiveFrom,
            pr.BaseDailySalary,
            pr.AttractionAllowance,
            pr.SupervisionAllowance,
            pr.TransportationAllowanceNet,
            pr.ChildrenCount,
            pr.MaritalStatus,
            pr.ShiftType,
            pr.ContractType,
            pr.IsTaxSubject,
            pr.CalculatedAmounts,
            pr.Amounts));

        return Result(response);
    }

    [HttpPut]
    [SwaggerOperation(OperationId = "SavePayrollRecord")]
    public async Task<ActionResult<Result<SavePayrollRecordCommandResponse>>> SavePayrollRecord(
        [FromBody] SavePayrollRecordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new SavePayrollRecordCommand(
            UserId: UserId,
            EmployeeId: request.EmployeeId,
            PersianYear: request.PersianYear,
            PersianMonth: request.PersianMonth,
            Work: request.Work
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{payrollRecordId:guid}")]
    [SwaggerOperation(OperationId = "DeletePayrollRecord")]
    public async Task<ActionResult<Result<bool>>> DeletePayrollRecord(
        [FromQuery] Guid employeeId,
        Guid payrollRecordId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeletePayrollRecordCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            PayrollRecordId: payrollRecordId
        ), cancellationToken);

        return Result(result);
    }

    [HttpPost("{payrollRecordId:guid}/mark-as-paid")]
    [SwaggerOperation(OperationId = "MarkPayrollRecordAsPaid")]
    public async Task<ActionResult<Result<bool>>> MarkPayrollRecordAsPaid(
        [FromQuery] Guid employeeId,
        Guid payrollRecordId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new MarkPayrollRecordAsPaidCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            PayrollRecordId: payrollRecordId
        ), cancellationToken);

        return Result(result);
    }
}
