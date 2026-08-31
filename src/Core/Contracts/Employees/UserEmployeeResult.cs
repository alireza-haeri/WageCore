namespace Core.Contracts.Employees;

public record UserEmployeeResult(
    Guid EmployeeId,
    string PersonalCode,
    string FullName,
    string WorkshopName,
    string DepartmentName,
    string NationalCode,
    DateOnly HireDate,
    string? JobTitle,
    EmployeeStatus Status,
    Region Region);
