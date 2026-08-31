namespace Application.Features.Workshops;

public record CreateWorkshopCommand(
    Guid UserId,
    string Name,
    string Address,
    DateOnly RegistrationDate,
    string NationalId,
    string? PostalCode = null)
    : IRequest<Result<CreateWorkshopCommandResponse>>;

public record CreateWorkshopCommandResponse(Guid WorkshopId);