namespace Web.Api.Controllers.SalaryDecrees.Contracts;

public record CreateSalaryDecreeRequest(
    Guid EmployeeId,
    SalaryDecreeRequest SalaryProfile);
