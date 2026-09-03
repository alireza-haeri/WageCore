using Core.Contracts.Employees;

namespace Application.Features.Employees;

public record GetUserEmployeeForEditQuery(Guid UserId, Guid EmployeeId)
    : IRequest<Result<GetUserEmployeeForEditQueryResponse>>;

public record GetUserEmployeeForEditQueryResponse(
    Guid WorkshopId,
    Guid DepartmentId,
    string PersonalCode,
    string FullName,
    string NationalCode,
    string FatherName,
    EmployeeGender Gender,
    DateOnly HireDate,
    string PhoneNumber,
    string? JobTitle,
    Region Region,
    int? LeaveUsedInCurrentYear,
    int? NetWorkedDaysBeforeCurrentMonth,
    int? CarriedOverLeaveFromPreviousYear,
    List<EmployeeBankAccountDto> BankAccounts);
