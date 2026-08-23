namespace Core.Contracts.Workshops;

public record UserWorkshopByIdResult(
    string Name,
    string Address,
    WorkshopRegion Region,
    DateOnly RegistrationDate,
    string NationalId,
    string? PostalCode = null);