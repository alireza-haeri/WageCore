namespace Core.Contracts.Employees;

public record EmployeeDto(
    Guid DepartmentId,
    string PersonalCode,
    string FullName,
    string NationalCode,
    string BirthCertificateNumber,
    string FatherName,
    EmployeeGender? Gender,
    EmployeeMaritalStatus? MaritalStatus,
    int? ChildrenCount,
    DateOnly? WorkshopRegistrationDate,
    DateOnly? HireDate,
    string PhoneNumber,
    string? JobTitle,
    bool IsTaxSubject);
