namespace Application.Features.SalaryDecrees;

public class GetSalaryDecreeForEditQueryHandler(ISalaryDecreeQuery salaryDecreeQuery)
    : IRequestHandler<GetSalaryDecreeForEditQuery, Result<GetSalaryDecreeForEditQueryResponse>>
{
    public async Task<Result<GetSalaryDecreeForEditQueryResponse>> Handle(
        GetSalaryDecreeForEditQuery request,
        CancellationToken cancellationToken)
    {
        var salaryProfile = await salaryDecreeQuery.GetSalaryDecreeByIdAsync(
            request.UserId,
            request.SalaryDecreeId,
            cancellationToken);

        if (salaryProfile is null)
            return Result<GetSalaryDecreeForEditQueryResponse>.NotfoundFailure(
                "پروفایل حقوق کارمند مورد نظر یافت نشد.");

        return Result<GetSalaryDecreeForEditQueryResponse>.Success(
            new GetSalaryDecreeForEditQueryResponse(
                request.SalaryDecreeId,
                salaryProfile.EmployeeId,
                salaryProfile.EffectiveFrom,
                salaryProfile.BaseDailySalary,
                salaryProfile.AttractionAllowance,
                salaryProfile.SupervisionAllowance,
                salaryProfile.ShiftType,
                salaryProfile.ContractType,
                salaryProfile.HousingAllowance,
                salaryProfile.FoodAllowance,
                salaryProfile.TransportationAllowanceNet,
                salaryProfile.KaranehAmountNet,
                salaryProfile.MaritalStatus,
                salaryProfile.ChildrenCount,
                salaryProfile.IsTaxSubject,
                salaryProfile.InsuranceNumber,
                salaryProfile.SocialSecurityContractRow,
                salaryProfile.PositionInInsuranceList,
                salaryProfile.IsSubjectTo7PercentInsurance,
                salaryProfile.IsSubjectTo20PercentInsurance,
                salaryProfile.IsSubjectTo3PercentInsurance,
                salaryProfile.IsSubjectTo4PercentInsurance,
                salaryProfile.InsuranceCalculationProfile));
    }
}
