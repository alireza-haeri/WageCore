namespace Core.Contracts.Employees;

public record UserEmployeeByIdResult(
    Guid WorkshopId,
    Guid DepartmentId,
    string PersonalCode,
    string FullName,
    string NationalCode,
    string BirthCertificateNumber,
    string FatherName,
    EmployeeGender Gender,
    EmployeeMaritalStatus MaritalStatus,
    int ChildrenCount,
    DateOnly HireDate,
    string PhoneNumber,
    string? JobTitle,
    bool IsTaxSubject,
    string InsuranceNumber,
    string? SocialSecurityContractRow,
    string PositionInInsuranceList,
    bool IsSubjectTo7PercentInsurance,
    bool IsSubjectTo20PercentInsurance,
    bool IsSubjectTo3PercentInsurance,
    InsuranceCalculationProfile InsuranceCalculationProfile);
