namespace Web.Api.Controllers.SalaryDecrees.Contracts;

public record SalaryDecreeRequest(
    PersianDate EffectiveFrom,
    decimal BaseDailySalary,
    decimal? AttractionAllowance,
    decimal? SupervisionAllowance,
    ShiftType ShiftType,
    ContractType ContractType,
    decimal? TransportationAllowanceNet,
    EmployeeMaritalStatus MaritalStatus,
    int ChildrenCount,
    bool IsTaxSubject,
    EmployeeInsuranceRequest Insurance);
