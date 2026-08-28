namespace Core.Contracts.Employees;

public record EmployeeSalaryProfileResult(
    Guid EmployeeSalaryProfileId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    string WorkshopName,
    string DepartmentName,
    DateOnly EffectiveFrom,
    decimal BaseMonthlySalary,
    EmployeeSalaryProfileStatus Status);
