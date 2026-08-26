using Core.Abstractions.Repositories.Employees;

namespace Application.Features.Employees;

public class GetUserEmployeeForEditQueryHandler(IEmployeeQuery employeeQuery)
    : IRequestHandler<GetUserEmployeeForEditQuery, Result<GetUserEmployeeForEditQueryResponse>>
{
    public async Task<Result<GetUserEmployeeForEditQueryResponse>> Handle(GetUserEmployeeForEditQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await employeeQuery.GetUserEmployeeByIdAsync(request.UserId, request.EmployeeId, cancellationToken);
        if (employee is null)
            return Result<GetUserEmployeeForEditQueryResponse>.NotfoundFailure("کارمند مورد نظر یافت نشد.");

        return Result<GetUserEmployeeForEditQueryResponse>.Success(
            new GetUserEmployeeForEditQueryResponse(
                employee.WorkshopId,
                employee.DepartmentId,
                employee.PersonalCode,
                employee.FullName,
                employee.NationalCode,
                employee.BirthCertificateNumber,
                employee.FatherName,
                employee.Gender,
                employee.MaritalStatus,
                employee.ChildrenCount,
                employee.HireDate,
                employee.PhoneNumber,
                employee.JobTitle,
                employee.IsTaxSubject,
                employee.InsuranceNumber,
                employee.SocialSecurityContractRow,
                employee.PositionInInsuranceList,
                employee.IsSubjectTo7PercentInsurance,
                employee.IsSubjectTo20PercentInsurance,
                employee.IsSubjectTo3PercentInsurance,
                employee.InsuranceCalculationProfile
            )
        );
    }
}
