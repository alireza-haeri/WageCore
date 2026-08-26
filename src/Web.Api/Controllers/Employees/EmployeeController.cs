using Shared.Web.DateTimeHandling;
using Core.Contracts.Employees;

namespace Web.Api.Controllers.Employees;

[Tags("Employee")]
[Route("api/v1/employees")]
public class EmployeeController(IMediator mediator) : BaseController
{
    [HttpPost]
    [SwaggerOperation(OperationId = "CreateEmployee")]
    public async Task<ActionResult<Result<CreateEmployeeCommandResponse>>> CreateEmployee(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateEmployeeCommand(
            UserId: UserId,
            WorkshopId: request.WorkshopId,
            Employee: new EmployeeDto(
                request.Employee.DepartmentId,
                request.Employee.PersonalCode,
                request.Employee.FullName,
                request.Employee.NationalCode,
                request.Employee.BirthCertificateNumber,
                request.Employee.FatherName,
                request.Employee.Gender,
                request.Employee.MaritalStatus,
                request.Employee.ChildrenCount,
                request.Employee.HireDate.ToDateOnly(),
                request.Employee.PhoneNumber,
                request.Employee.JobTitle,
                request.Employee.IsTaxSubject),
            Insurance: new EmployeeInsuranceDto(
                request.Insurance.InsuranceNumber,
                request.Insurance.SocialSecurityContractRow,
                request.Insurance.PositionInInsuranceList,
                request.Insurance.IsSubjectTo7PercentInsurance,
                request.Insurance.IsSubjectTo20PercentInsurance,
                request.Insurance.IsSubjectTo3PercentInsurance,
                request.Insurance.InsuranceCalculationProfile),
            BankAccounts: request.BankAccounts
                .Select(x => new EmployeeBankAccountDto(x.Title, x.Iban, x.Id))
                .ToList()
        ), cancellationToken);

        return Result(result);
    }

    [HttpPut("{employeeId:guid}")]
    [SwaggerOperation(OperationId = "UpdateEmployee")]
    public async Task<ActionResult<Result<bool>>> UpdateEmployee(
        [FromBody] UpdateEmployeeRequest request,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateEmployeeCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            Employee: new EmployeeDto(
                request.Employee.DepartmentId,
                request.Employee.PersonalCode,
                request.Employee.FullName,
                request.Employee.NationalCode,
                request.Employee.BirthCertificateNumber,
                request.Employee.FatherName,
                request.Employee.Gender,
                request.Employee.MaritalStatus,
                request.Employee.ChildrenCount,
                request.Employee.HireDate.ToDateOnly(),
                request.Employee.PhoneNumber,
                request.Employee.JobTitle,
                request.Employee.IsTaxSubject),
            Insurance: new EmployeeInsuranceDto(
                request.Insurance.InsuranceNumber,
                request.Insurance.SocialSecurityContractRow,
                request.Insurance.PositionInInsuranceList,
                request.Insurance.IsSubjectTo7PercentInsurance,
                request.Insurance.IsSubjectTo20PercentInsurance,
                request.Insurance.IsSubjectTo3PercentInsurance,
                request.Insurance.InsuranceCalculationProfile),
            BankAccounts: request.BankAccounts
                .Select(x => new EmployeeBankAccountDto(x.Title, x.Iban, x.Id))
                .ToList()
        ), cancellationToken);

        return Result(result);
    }

    [HttpDelete("{employeeId:guid}")]
    [SwaggerOperation(OperationId = "DeleteEmployee")]
    public async Task<ActionResult<Result<bool>>> DeleteEmployee(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteEmployeeCommand(
            UserId: UserId,
            EmployeeId: employeeId
        ), cancellationToken);

        return Result(result);
    }

    [HttpPost("{employeeId:guid}/terminate")]
    [SwaggerOperation(OperationId = "TerminateEmployee")]
    public async Task<ActionResult<Result<bool>>> TerminateEmployee(
        [FromBody] TerminateEmployeeRequest request,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new TerminateEmployeeCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            TerminationDate: request.TerminationDate.ToDateOnly()
        ), cancellationToken);

        return Result(result);
    }

    [HttpPost("{employeeId:guid}/rehire")]
    [SwaggerOperation(OperationId = "RehireEmployee")]
    public async Task<ActionResult<Result<bool>>> RehireEmployee(
        [FromBody] RehireEmployeeRequest request,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RehireEmployeeCommand(
            UserId: UserId,
            EmployeeId: employeeId,
            DepartmentId: request.DepartmentId,
            HireDate: request.HireDate.ToDateOnly()
        ), cancellationToken);

        return Result(result);
    }

    [HttpGet]
    [SwaggerOperation(OperationId = "GetUserEmployees")]
    public async Task<ActionResult<Result<PagedResult<GetUserEmployeesResponse>>>> GetUserEmployees(
        [FromQuery] GetUserEmployeesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserEmployeesQuery(
            UserId: UserId,
            Pagination: request.Pagination,
            Search: request.Search,
            WorkshopId: request.WorkshopId,
            DepartmentId: request.DepartmentId,
            Status: request.Status
        ), cancellationToken);

        var response = result
            .Map(paged => paged
                .Map(e => new GetUserEmployeesResponse(
                    e.Id,
                    e.PersonalCode,
                    e.FullName,
                    e.WorkshopName,
                    e.DepartmentName,
                    e.NationalCode,
                    PersianDate.FromDateOnly(e.HireDate).ToDisplay(UserPersianDateFormat),
                    e.JobTitle,
                    e.Status)
                )
            );

        return Result(response);
    }

    [HttpGet("{employeeId:guid}/edit")]
    [SwaggerOperation(OperationId = "GetEmployeeForEdit")]
    public async Task<ActionResult<Result<GetEmployeeForEditResponse>>> GetEmployeeForEdit(
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserEmployeeForEditQuery(
            UserId: UserId,
            EmployeeId: employeeId
        ), cancellationToken);

        var response = result.Map(e => new GetEmployeeForEditResponse(
            e.WorkshopId,
            e.DepartmentId,
            e.PersonalCode,
            e.FullName,
            e.NationalCode,
            e.BirthCertificateNumber,
            e.FatherName,
            e.Gender,
            e.MaritalStatus,
            e.ChildrenCount,
            PersianDate.ToRawValue(e.HireDate),
            e.PhoneNumber,
            e.JobTitle,
            e.IsTaxSubject,
            e.InsuranceNumber,
            e.SocialSecurityContractRow,
            e.PositionInInsuranceList,
            e.IsSubjectTo7PercentInsurance,
            e.IsSubjectTo20PercentInsurance,
            e.IsSubjectTo3PercentInsurance,
            e.InsuranceCalculationProfile,
            e.BankAccounts
                .Select(x => new EmployeeBankAccountResponse(
                    x.Title,
                    $"IR{x.Iban}",
                    x.Id))
                .ToList()
        ));

        return Result(response);
    }
}
