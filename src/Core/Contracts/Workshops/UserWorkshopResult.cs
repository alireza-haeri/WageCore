namespace Core.Contracts.Workshops;

public record UserWorkshopResult(
    Guid WorkshopId,
    string Name,
    string Address,
    string NationalId,
    DateOnly RegistrationDate,
    int EmployeesCount,
    int DepartmentsCount,
    string SocialSecurityNumber,
    string? EconomicCode = null);