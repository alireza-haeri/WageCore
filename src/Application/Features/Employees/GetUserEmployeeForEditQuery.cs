using Core.Contracts.Employees;

namespace Application.Features.Employees;

public record GetUserEmployeeForEditQuery(Guid UserId, Guid EmployeeId)
    : IRequest<Result<GetUserEmployeeForEditQueryResponse>>;

public record GetUserEmployeeForEditQueryResponse(
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
    InsuranceCalculationProfile InsuranceCalculationProfile,
    List<EmployeeBankAccountDto> BankAccounts);
