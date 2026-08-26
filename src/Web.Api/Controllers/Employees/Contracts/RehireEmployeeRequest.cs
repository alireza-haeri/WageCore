namespace Web.Api.Controllers.Employees.Contracts;

public record RehireEmployeeRequest(
    Guid DepartmentId,
    PersianDate HireDate);
