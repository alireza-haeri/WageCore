namespace Core.Contracts.Employees;

public record EmployeeSalaryProfileResult(
    Guid EmployeeSalaryProfileId,
    Guid EmployeeId,
    string EmployeeName,
    string PersonalCode,
    DateOnly EffectiveFrom,
    decimal BaseMonthlySalary,
    EmployeeSalaryProfileStatus Status);
