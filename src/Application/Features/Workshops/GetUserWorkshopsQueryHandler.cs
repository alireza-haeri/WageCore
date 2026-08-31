using Core.Abstractions.Repositories.Workshops;
using Core.Contracts;

namespace Application.Features.Workshops;

public class GetUserWorkshopsQueryHandler(IWorkshopQuery workshopQuery)
    : IRequestHandler<GetUserWorkshopsQuery, Result<PagedResult<GetUserWorkshopsQueryResponse>>>
{
    public async Task<Result<PagedResult<GetUserWorkshopsQueryResponse>>> Handle(GetUserWorkshopsQuery request,
        CancellationToken cancellationToken)
    {
        var userWorkshopsPaged = await workshopQuery.GetUserWorkshopsAsync(
            request.UserId,
            request.Pagination,
            request.SearchName,
            cancellationToken);

        var response = userWorkshopsPaged.Map(x =>
            new GetUserWorkshopsQueryResponse(
                x.WorkshopId, x.Name, x.Address, x.NationalId,
                x.RegistrationDate, x.EmployeesCount, x.DepartmentsCount)
        );

        return Result<PagedResult<GetUserWorkshopsQueryResponse>>.Success(response);
    }
}