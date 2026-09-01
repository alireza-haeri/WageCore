namespace Web.Api.Controllers.Workshops.Contracts;

public record GetUserWorkshopsRequest(
    PaginationDto Pagination,
    string? SearchName = null
);

public record GetUserWorkshopsResponse(
    Guid Id,
    string Name,
    string Address,
    string NationalId,
    string DisplayRegistrationDate,
    int EmployeesCount,
    int DepartmentsCount,
    string SocialSecurityNumber,
    string? EconomicCode = null);