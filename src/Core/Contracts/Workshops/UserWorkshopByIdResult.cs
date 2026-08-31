namespace Core.Contracts.Workshops;

public record UserWorkshopByIdResult(
    string Name,
    string Address,
    DateOnly RegistrationDate,
    string NationalId,
    string SocialSecurityNumber,
    string? PostalCode = null,
    string? EconomicCode = null);