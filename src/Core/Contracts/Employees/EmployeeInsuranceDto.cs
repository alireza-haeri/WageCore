namespace Core.Contracts.Employees;

public record EmployeeInsuranceDto(
    string InsuranceNumber,
    string PositionInInsuranceList,
    bool IsSubjectTo7PercentInsurance,
    bool IsSubjectTo20PercentInsurance,
    bool IsSubjectTo3PercentInsurance,
    bool IsSubjectTo4PercentInsurance,
    InsuranceCalculationProfile? InsuranceCalculationProfile);
