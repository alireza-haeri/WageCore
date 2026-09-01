namespace Web.Api.Controllers.Employees.Contracts;

public record CreateEmployeeRequest(
    Guid WorkshopId,
    EmployeeInformationRequest Employee,
    List<EmployeeBankAccountRequest> BankAccounts);
