namespace Application.Features.Workshops;

public record GetUserWorkshopsNameQuery(Guid UserId)
    : IRequest<Result<GetUserWorkshopsNameQueryResponse>>;

public record GetUserWorkshopsNameQueryResponse(List<GetUserWorkshopsNameQueryResponseWorkshopName> WorkshopNames);

public record GetUserWorkshopsNameQueryResponseWorkshopName(Guid WorkshopId, string DisplayName);