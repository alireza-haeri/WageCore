namespace Core.Contracts.Departments;

public record UserDepartmentByIdResult(
    string Name,
    Guid WorkshopId);
