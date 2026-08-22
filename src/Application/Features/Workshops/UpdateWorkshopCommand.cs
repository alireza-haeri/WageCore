namespace Application.Features.Workshops;

public record UpdateWorkshopCommand(
    Guid UserId,
    Guid WorkshopId,
    string Name,
    string Address,
    WorkshopRegion Region,
    DateOnly RegistrationDate,
    string NationalId,
    string? PostalCode = null)
    : IRequest<Result<bool>>;