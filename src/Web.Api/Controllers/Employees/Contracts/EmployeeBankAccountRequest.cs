namespace Web.Api.Controllers.Employees.Contracts;

public record EmployeeBankAccountRequest(
    string? Title,
    string Iban,
    Guid? Id = null);

public record EmployeeBankAccountResponse(
    string? Title,
    string Iban,
    Guid? Id = null);
