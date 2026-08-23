namespace Web.Api.Controllers.Workshops.Contracts;

public record GetUserWorkshopsRequest(
    PaginationDto Pagination,
    string? SearchName = null,
    WorkshopRegion? Region = null
);

public record GetUserWorkshopsResponse(
    Guid Id,
    string Name,
    string Address,
    WorkshopRegion Region,
    string DisplayRegistrationDate,
    int EmployeesCount,
    int DepartmentsCount);