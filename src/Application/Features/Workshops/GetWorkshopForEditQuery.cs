namespace Application.Features.Workshops;

public record GetWorkshopForEditQuery(Guid UserId, Guid WorkshopId) : IRequest<Result<GetWorkshopForEditQueryResponse>>;
public record GetWorkshopForEditQueryResponse(
    string Name,
    string Address,
    WorkshopRegion Region,
    DateOnly RegistrationDate,
    string NationalId,
    string? PostalCode = null);