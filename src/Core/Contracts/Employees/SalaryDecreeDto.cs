namespace Core.Contracts.Employees;

public record SalaryDecreeDto(
    DateOnly? EffectiveFrom,
    decimal? BaseDailySalary,
    decimal? AttractionAllowance,
    decimal? SupervisionAllowance,
    ShiftType? ShiftType,
    ContractType? ContractType,
    decimal? HousingAllowance,
    decimal? FoodAllowance,
    decimal? TransportationAllowanceNet,
    decimal? KaranehAmountNet,
    EmployeeMaritalStatus? MaritalStatus,
    int? ChildrenCount,
    bool? IsTaxSubject,
    EmployeeInsuranceDto? Insurance);
