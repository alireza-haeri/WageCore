namespace Web.Api.Controllers.Employees.Contracts;

public record UpdateEmployeeRequest(
    EmployeeInformationRequest Employee,
    List<EmployeeBankAccountRequest> BankAccounts);
