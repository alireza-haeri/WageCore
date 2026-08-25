namespace Application.Features.Workshops;

public class GetWorkshopForEditQueryHandler(IWorkshopQuery workshopQuery)
: IRequestHandler<GetWorkshopForEditQuery,Result<GetWorkshopForEditQueryResponse>>
{
    public async Task<Result<GetWorkshopForEditQueryResponse>> Handle(GetWorkshopForEditQuery request, CancellationToken cancellationToken)
    {
        var workshop = await workshopQuery.GetUserWorkshopByIdAsync(request.UserId, request.WorkshopId, cancellationToken);
        if(workshop is null)
            return Result<GetWorkshopForEditQueryResponse>.NotfoundFailure("کارگاه مورد نظر یافت نشد.");
        
        return Result<GetWorkshopForEditQueryResponse>.Success(
            new GetWorkshopForEditQueryResponse(
                workshop.Name,
                workshop.Address,
                workshop.Region,
                workshop.RegistrationDate,
                workshop.NationalId,
                workshop.PostalCode
            )
        );
    }
}