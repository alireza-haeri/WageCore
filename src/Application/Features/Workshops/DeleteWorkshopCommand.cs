namespace Application.Features.Workshops;

public record DeleteWorkshopCommand(Guid UserId, Guid WorkshopId)
    : IRequest<Result<bool>>;