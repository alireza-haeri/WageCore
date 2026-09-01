namespace Web.Api.Controllers.SalaryDecrees.Contracts;

public record GetSalaryDecreeForEditResponse(
    Guid SalaryDecreeId,
    Guid EmployeeId,
    string EffectiveFrom,
    decimal BaseDailySalary,
    decimal? AttractionAllowance,
    decimal? SupervisionAllowance,
    ShiftType ShiftType,
    ContractType ContractType,
    decimal? TransportationAllowanceNet,
    EmployeeMaritalStatus MaritalStatus,
    int ChildrenCount,
    bool IsTaxSubject,
    string InsuranceNumber,
    string PositionInInsuranceList,
    bool IsSubjectTo7PercentInsurance,
    bool IsSubjectTo20PercentInsurance,
    bool IsSubjectTo3PercentInsurance,
    bool IsSubjectTo4PercentInsurance,
    InsuranceCalculationProfile InsuranceCalculationProfile);
