namespace Web.Api.Controllers.EmployeeSalaryProfiles.Contracts;

public record UpdateEmployeeSalaryProfileRequest(
    Guid EmployeeId,
    EmployeeSalaryProfileRequest SalaryProfile);
