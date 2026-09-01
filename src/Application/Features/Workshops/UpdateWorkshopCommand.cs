namespace Application.Features.Workshops;

public record UpdateWorkshopCommand(
    Guid UserId,
    Guid WorkshopId,
    string Name,
    string Address,
    DateOnly RegistrationDate,
    string NationalId,
    string SocialSecurityNumber,
    string? PostalCode = null,
    string? EconomicCode = null)
    : IRequest<Result<bool>>;