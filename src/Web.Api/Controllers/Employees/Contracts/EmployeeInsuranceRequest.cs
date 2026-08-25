namespace Web.Api.Controllers.Employees.Contracts;

public record EmployeeInsuranceRequest(
    string InsuranceNumber,
    string? SocialSecurityContractRow,
    string PositionInInsuranceList,
    bool IsSubjectTo7PercentInsurance,
    bool IsSubjectTo20PercentInsurance,
    bool IsSubjectTo3PercentInsurance,
    InsuranceCalculationProfile InsuranceCalculationProfile);
