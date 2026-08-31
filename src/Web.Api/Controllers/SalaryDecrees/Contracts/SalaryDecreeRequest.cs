namespace Web.Api.Controllers.SalaryDecrees.Contracts;

public record SalaryDecreeRequest(
    PersianDate EffectiveFrom,
    decimal BaseDailySalary,
    decimal? AttractionAllowance,
    decimal? SupervisionAllowance,
    ShiftType ShiftType,
    ContractType ContractType,
    decimal? HousingAllowance,
    decimal? FoodAllowance,
    decimal? TransportationAllowanceNet,
    decimal? KaranehAmountNet,
    EmployeeMaritalStatus MaritalStatus,
    int ChildrenCount,
    bool IsTaxSubject,
    EmployeeInsuranceRequest Insurance);
