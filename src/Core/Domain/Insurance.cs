namespace Core.Domain;

public class Insurance
{
    public string InsuranceNumber { get; private set; } = null!;
    public string PositionInInsuranceList { get; private set; } = null!;
    public bool IsSubjectTo7PercentInsurance { get; private set; }
    public bool IsSubjectTo20PercentInsurance { get; private set; }
    public bool IsSubjectTo3PercentInsurance { get; private set; }
    public bool IsSubjectTo4PercentInsurance { get; private set; }
    public InsuranceCalculationProfile InsuranceCalculationProfile { get; private set; }

    public static DomainResult<Insurance> Create(EmployeeInsuranceDto? insurance)
    {
        var validationResult = Validate(insurance);
        if (!validationResult.IsSuccess)
            return DomainResult<Insurance>.Failure(validationResult.ErrorMessage!);

        return DomainResult<Insurance>.Success(new Insurance
        {
            InsuranceNumber = insurance!.InsuranceNumber,
            PositionInInsuranceList = insurance.PositionInInsuranceList,
            IsSubjectTo7PercentInsurance = insurance.IsSubjectTo7PercentInsurance,
            IsSubjectTo20PercentInsurance = insurance.IsSubjectTo20PercentInsurance,
            IsSubjectTo3PercentInsurance = insurance.IsSubjectTo3PercentInsurance,
            IsSubjectTo4PercentInsurance = insurance.IsSubjectTo4PercentInsurance,
            InsuranceCalculationProfile = insurance.InsuranceCalculationProfile!.Value
        });
    }

    public DomainResult Update(EmployeeInsuranceDto? insurance)
    {
        var validationResult = Validate(insurance);
        if (!validationResult.IsSuccess)
            return validationResult;

        InsuranceNumber = insurance!.InsuranceNumber;
        PositionInInsuranceList = insurance.PositionInInsuranceList;
        IsSubjectTo7PercentInsurance = insurance.IsSubjectTo7PercentInsurance;
        IsSubjectTo20PercentInsurance = insurance.IsSubjectTo20PercentInsurance;
        IsSubjectTo3PercentInsurance = insurance.IsSubjectTo3PercentInsurance;
        IsSubjectTo4PercentInsurance = insurance.IsSubjectTo4PercentInsurance;
        InsuranceCalculationProfile = insurance.InsuranceCalculationProfile!.Value;

        return DomainResult.Success();
    }

    private static DomainResult Validate(EmployeeInsuranceDto? insurance)
    {
        if (insurance is null)
            return DomainResult.Failure("اطلاعات بیمه نمیتواند خالی باشد.");

        if (string.IsNullOrWhiteSpace(insurance.InsuranceNumber))
            return DomainResult.Failure("شماره بیمه نمیتواند خالی باشد.");

        if (insurance.InsuranceNumber.Length > 20)
            return DomainResult.Failure("شماره بیمه نمیتواند بیشتر از 20 حرف باشد.");

        if (string.IsNullOrWhiteSpace(insurance.PositionInInsuranceList))
            return DomainResult.Failure("سمت در لیست بیمه نمیتواند خالی باشد.");

        if (insurance.PositionInInsuranceList.Length > 100)
            return DomainResult.Failure("سمت در لیست بیمه نمیتواند بیشتر از 100 حرف باشد.");

        if (insurance.InsuranceCalculationProfile is null)
            return DomainResult.Failure("پروفایل محاسبه بیمه نمیتواند خالی باشد.");

        return DomainResult.Success();
    }
}
