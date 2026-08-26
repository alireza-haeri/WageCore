namespace Web.Api.Controllers.Employees.Contracts;

public record UpdateEmployeeRequest(
    EmployeeInformationRequest Employee,
    EmployeeInsuranceRequest Insurance,
    List<EmployeeBankAccountRequest> BankAccounts);
