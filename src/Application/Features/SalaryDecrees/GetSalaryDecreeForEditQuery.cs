namespace Application.Features.SalaryDecrees;

public record GetSalaryDecreeForEditQuery(Guid UserId, Guid SalaryDecreeId)
    : IRequest<Result<GetSalaryDecreeForEditQueryResponse>>;

public record GetSalaryDecreeForEditQueryResponse(
    Guid SalaryDecreeId,
    Guid EmployeeId,
    DateOnly EffectiveFrom,
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
    string InsuranceNumber,
    string? SocialSecurityContractRow,
    string PositionInInsuranceList,
    bool IsSubjectTo7PercentInsurance,
    bool IsSubjectTo20PercentInsurance,
    bool IsSubjectTo3PercentInsurance,
    bool IsSubjectTo4PercentInsurance,
    InsuranceCalculationProfile InsuranceCalculationProfile);
