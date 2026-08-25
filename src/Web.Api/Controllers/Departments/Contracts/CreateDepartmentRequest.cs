namespace Web.Api.Controllers.Departments.Contracts;

public record CreateDepartmentRequest(
    Guid WorkshopId,
    string Name);
