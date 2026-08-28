namespace Web.Api.Controllers.EmployeeSalaryProfiles.Contracts;

public record CreateEmployeeSalaryProfileRequest(
    Guid EmployeeId,
    EmployeeSalaryProfileRequest SalaryProfile);
