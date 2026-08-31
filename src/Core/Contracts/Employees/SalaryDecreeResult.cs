namespace Core.Contracts.Employees;

public record SalaryDecreeResult(
    Guid SalaryDecreeId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    string WorkshopName,
    string DepartmentName,
    DateOnly EffectiveFrom,
    decimal BaseDailySalary,
    SalaryDecreeStatus Status);
