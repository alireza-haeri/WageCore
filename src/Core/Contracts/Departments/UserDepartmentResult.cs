namespace Core.Contracts.Departments;

public record UserDepartmentResult(
    Guid DepartmentId,
    string Name,
    Guid WorkshopId,
    string WorkshopName,
    int EmployeesCount);
