namespace Application.Features.Workshops;

public record CreateWorkshopCommand(
    Guid UserId,
    string Name,
    string Address,
    DateOnly RegistrationDate,
    string NationalId,
    string SocialSecurityNumber,
    string? PostalCode = null,
    string? EconomicCode = null)
    : IRequest<Result<CreateWorkshopCommandResponse>>;

public record CreateWorkshopCommandResponse(Guid WorkshopId);