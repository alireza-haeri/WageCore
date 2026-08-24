namespace Core.Contracts.Workshops;

public record UserWorkshopResult(
    Guid WorkshopId,
    string Name,
    string Address,
    string NationalId,
    WorkshopRegion Region,
    DateOnly RegistrationDate,
    int EmployeesCount,
    int DepartmentsCount);