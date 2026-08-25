namespace Core.Contracts.Employees;

public record EmployeeInsuranceDto(
    string InsuranceNumber,
    string? SocialSecurityContractRow,
    string PositionInInsuranceList,
    bool IsSubjectTo7PercentInsurance,
    bool IsSubjectTo20PercentInsurance,
    bool IsSubjectTo3PercentInsurance,
    InsuranceCalculationProfile? InsuranceCalculationProfile);
