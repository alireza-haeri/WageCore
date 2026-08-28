namespace Web.Api.Controllers.EmployeeSalaryProfiles.Contracts;

public record GetEmployeeSalaryProfileForEditResponse(
    Guid EmployeeSalaryProfileId,
    Guid EmployeeId,
    string EffectiveFrom,
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
