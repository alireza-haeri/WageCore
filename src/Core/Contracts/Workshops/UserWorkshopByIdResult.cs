namespace Core.Contracts.Workshops;

public record UserWorkshopByIdResult(
    string Name,
    string Address,
    DateOnly RegistrationDate,
    string NationalId,
    string? PostalCode = null,
    string SocialSecurityNumber = null!,
    string? EconomicCode = null);