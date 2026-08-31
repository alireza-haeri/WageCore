namespace Application.Features.Workshops;

public record GetWorkshopForEditQuery(Guid UserId, Guid WorkshopId) : IRequest<Result<GetWorkshopForEditQueryResponse>>;
public record GetWorkshopForEditQueryResponse(
    string Name,
    string Address,
    DateOnly RegistrationDate,
    string NationalId,
    string? PostalCode = null,
    string SocialSecurityNumber = null!,
    string? EconomicCode = null);