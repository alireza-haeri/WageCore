namespace Web.Api.Controllers.SalaryDecrees.Contracts;

public record UpdateSalaryDecreeRequest(
    Guid EmployeeId,
    SalaryDecreeRequest SalaryProfile);
