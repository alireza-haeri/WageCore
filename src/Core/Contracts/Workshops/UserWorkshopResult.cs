namespace Core.Contracts.Workshops;

public record UserWorkshopResult(
    Guid WorkshopId,
    string Name,
    string Address,
    WorkshopRegion Region,
    DateOnly RegistrationDate,
    int EmployeesCount,
    int DepartmentsCount);