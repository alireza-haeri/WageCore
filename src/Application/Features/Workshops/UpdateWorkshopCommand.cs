namespace Application.Features.Workshops;

public record UpdateWorkshopCommand(
    Guid UserId,
    Guid WorkshopId,
    string Name,
    string Address,
    DateOnly RegistrationDate,
    string NationalId,
    string? PostalCode = null)
    : IRequest<Result<bool>>;