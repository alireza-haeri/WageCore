namespace Application.Features.Workshops;

public class GetUserWorkshopsNameQueryHandler(IWorkshopQuery workshopQuery)
    : IRequestHandler<GetUserWorkshopsNameQuery, Result<GetUserWorkshopsNameQueryResponse>>
{
    public async Task<Result<GetUserWorkshopsNameQueryResponse>> Handle(GetUserWorkshopsNameQuery request,
        CancellationToken cancellationToken)
    {
        var userWorkshopsName = await workshopQuery.GetUserWorkshopsNameAsync(request.UserId, cancellationToken);
        var response = userWorkshopsName
            .Select(u => new GetUserWorkshopsNameQueryResponseWorkshopName(u.WorkshopId, u.DisplayName))
            .ToList();

        return Result<GetUserWorkshopsNameQueryResponse>.Success(new GetUserWorkshopsNameQueryResponse(response));
    }
}