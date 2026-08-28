namespace Application.Features.EmployeeSalaryProfiles;

public class GetEmployeeSalaryProfileForEditQueryHandler(IEmployeeSalaryProfileQuery employeeSalaryProfileQuery)
    : IRequestHandler<GetEmployeeSalaryProfileForEditQuery, Result<GetEmployeeSalaryProfileForEditQueryResponse>>
{
    public async Task<Result<GetEmployeeSalaryProfileForEditQueryResponse>> Handle(
        GetEmployeeSalaryProfileForEditQuery request,
        CancellationToken cancellationToken)
    {
        var salaryProfile = await employeeSalaryProfileQuery.GetEmployeeSalaryProfileByIdAsync(
            request.UserId,
            request.EmployeeSalaryProfileId,
            cancellationToken);

        if (salaryProfile is null)
            return Result<GetEmployeeSalaryProfileForEditQueryResponse>.NotfoundFailure(
                "پروفایل حقوق کارمند مورد نظر یافت نشد.");

        return Result<GetEmployeeSalaryProfileForEditQueryResponse>.Success(
            new GetEmployeeSalaryProfileForEditQueryResponse(
                request.EmployeeSalaryProfileId,
                salaryProfile.EmployeeId,
                salaryProfile.EffectiveFrom,
                salaryProfile.BaseMonthlySalary,
                salaryProfile.AttractionAllowance,
                salaryProfile.SupervisionAllowance,
                salaryProfile.SeniorityBaseApplicationMode,
                salaryProfile.SeniorityBaseCalculationMethod,
                salaryProfile.YearEndSeniorityMode,
                salaryProfile.ShiftType,
                salaryProfile.HousingAllowance,
                salaryProfile.FoodAllowance,
                salaryProfile.ChildAllowancePerChild,
                salaryProfile.TransportationAllowanceNet,
                salaryProfile.KaranehAmountNet));
    }
}
