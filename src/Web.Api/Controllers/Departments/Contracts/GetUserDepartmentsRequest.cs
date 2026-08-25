namespace Web.Api.Controllers.Departments.Contracts;

public record GetUserDepartmentsRequest(
    PaginationDto Pagination,
    string? SearchName = null,
    Guid? WorkshopId = null
);
