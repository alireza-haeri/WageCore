namespace Core.Contracts.Employees;

public record EmployeeSalaryProfileDto(
    DateOnly? EffectiveFrom,
    decimal? BaseMonthlySalary,
    decimal? AttractionAllowance,
    decimal? SupervisionAllowance,
    SeniorityBaseApplicationMode? SeniorityBaseApplicationMode,
    SeniorityBaseCalculationMethod? SeniorityBaseCalculationMethod,
    YearEndSeniorityMode? YearEndSeniorityMode,
    ShiftType? ShiftType,
    decimal? HousingAllowance,
    decimal? FoodAllowance,
    decimal? ChildAllowancePerChild,
    decimal? TransportationAllowanceNet,
    decimal? KaranehAmountNet);
