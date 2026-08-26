namespace Web.Api.Controllers.Employees.Contracts;

public record EmployeeInformationRequest(
    Guid DepartmentId,
    string PersonalCode,
    string FullName,
    string NationalCode,
    string BirthCertificateNumber,
    string FatherName,
    EmployeeGender Gender,
    EmployeeMaritalStatus MaritalStatus,
    int ChildrenCount,
    PersianDate HireDate,
    string PhoneNumber,
    string? JobTitle,
    bool IsTaxSubject);
