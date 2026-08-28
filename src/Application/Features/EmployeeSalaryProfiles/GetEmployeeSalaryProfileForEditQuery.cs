namespace Application.Features.EmployeeSalaryProfiles;

public record GetEmployeeSalaryProfileForEditQuery(Guid UserId, Guid EmployeeSalaryProfileId)
    : IRequest<Result<GetEmployeeSalaryProfileForEditQueryResponse>>;

public record GetEmployeeSalaryProfileForEditQueryResponse(
    Guid EmployeeSalaryProfileId,
    Guid EmployeeId,
    DateOnly EffectiveFrom,
    decimal BaseMonthlySalary,
    decimal? AttractionAllowance,
    decimal? SupervisionAllowance,
    SeniorityBaseApplicationMode SeniorityBaseApplicationMode,
    SeniorityBaseCalculationMethod? SeniorityBaseCalculationMethod,
    YearEndSeniorityMode YearEndSeniorityMode,
    ShiftType ShiftType,
    decimal? HousingAllowance,
    decimal? FoodAllowance,
    decimal? ChildAllowancePerChild,
    decimal? TransportationAllowanceNet,
    decimal? KaranehAmountNet);
