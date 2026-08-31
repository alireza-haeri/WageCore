namespace Core.Domain;

public class SalaryDecree
{
    public const string TableName = "SalaryDecrees";

    public Guid Id { get; private init; }
    public Guid EmployeeId { get; private init; }
    public DateOnly EffectiveFrom { get; private set; }
    public decimal BaseDailySalary { get; private set; }
    public decimal? AttractionAllowance { get; private set; }
    public decimal? SupervisionAllowance { get; private set; }
    public ShiftType ShiftType { get; private set; }
    public ContractType ContractType { get; private set; }
    public decimal? TransportationAllowanceNet { get; private set; }
    public EmployeeMaritalStatus MaritalStatus { get; private set; }
    public int ChildrenCount { get; private set; }
    public bool IsTaxSubject { get; private set; }
    public Insurance Insurance { get; private set; } = null!;

    public static DomainResult<SalaryDecree> Create(
        Guid salaryProfileId,
        Guid employeeId,
        DateOnly? employeeHireDate,
        DateOnly? latestExistingEffectiveFrom,
        decimal? minimumMonthlySalary,
        SalaryDecreeDto? salaryProfile)
    {
        var validationResult = Validate(
            salaryProfileId,
            employeeId,
            employeeHireDate,
            latestExistingEffectiveFrom,
            minimumMonthlySalary,
            salaryProfile);

        if (!validationResult.IsSuccess)
            return DomainResult<SalaryDecree>.Failure(validationResult.ErrorMessage!);

        var insuranceResult = Insurance.Create(salaryProfile!.Insurance);
        if (!insuranceResult.IsSuccess)
            return DomainResult<SalaryDecree>.Failure(insuranceResult.ErrorMessage!);

        return DomainResult<SalaryDecree>.Success(new SalaryDecree
        {
            Id = salaryProfileId,
            EmployeeId = employeeId,
            EffectiveFrom = salaryProfile!.EffectiveFrom!.Value,
            BaseDailySalary = salaryProfile.BaseDailySalary!.Value,
            AttractionAllowance = salaryProfile.AttractionAllowance,
            SupervisionAllowance = salaryProfile.SupervisionAllowance,
            ShiftType = salaryProfile.ShiftType!.Value,
            ContractType = salaryProfile.ContractType!.Value,
            TransportationAllowanceNet = salaryProfile.TransportationAllowanceNet,
            MaritalStatus = salaryProfile.MaritalStatus!.Value,
            ChildrenCount = salaryProfile.ChildrenCount!.Value,
            IsTaxSubject = salaryProfile.IsTaxSubject!.Value,
            Insurance = insuranceResult.Response
        });
    }

    public static DomainResult<SalaryDecree> Create(
        Guid employeeId,
        DateOnly? employeeHireDate,
        DateOnly? latestExistingEffectiveFrom,
        decimal? minimumMonthlySalary,
        SalaryDecreeDto? salaryProfile) =>
        Create(
            Guid.NewGuid(),
            employeeId,
            employeeHireDate,
            latestExistingEffectiveFrom,
            minimumMonthlySalary,
            salaryProfile);

    public DomainResult Update(
        DateOnly? employeeHireDate,
        DateOnly? latestExistingEffectiveFrom,
        decimal? minimumMonthlySalary,
        SalaryDecreeDto? salaryProfile)
    {
        var validationResult = ValidateCommon(
            employeeHireDate,
            latestExistingEffectiveFrom,
            minimumMonthlySalary,
            salaryProfile);

        if (!validationResult.IsSuccess)
            return validationResult;

        var insuranceResult = Insurance.Create(salaryProfile!.Insurance);
        if (!insuranceResult.IsSuccess)
            return insuranceResult;

        EffectiveFrom = salaryProfile!.EffectiveFrom!.Value;
        BaseDailySalary = salaryProfile.BaseDailySalary!.Value;
        AttractionAllowance = salaryProfile.AttractionAllowance;
        SupervisionAllowance = salaryProfile.SupervisionAllowance;
        ShiftType = salaryProfile.ShiftType!.Value;
        ContractType = salaryProfile.ContractType!.Value;
        TransportationAllowanceNet = salaryProfile.TransportationAllowanceNet;
        MaritalStatus = salaryProfile.MaritalStatus!.Value;
        ChildrenCount = salaryProfile.ChildrenCount!.Value;
        IsTaxSubject = salaryProfile.IsTaxSubject!.Value;
        Insurance = insuranceResult.Response;

        return DomainResult.Success();
    }

    private static DomainResult Validate(
        Guid salaryProfileId,
        Guid employeeId,
        DateOnly? employeeHireDate,
        DateOnly? latestExistingEffectiveFrom,
        decimal? minimumMonthlySalary,
        SalaryDecreeDto? salaryProfile)
    {
        if (salaryProfileId == Guid.Empty)
            return DomainResult.Failure("شناسه پروفایل حقوق کارمند نمیتواند خالی باشد.");

        if (employeeId == Guid.Empty)
            return DomainResult.Failure("شناسه کارمند نمیتواند خالی باشد.");

        return ValidateCommon(employeeHireDate, latestExistingEffectiveFrom, minimumMonthlySalary, salaryProfile);
    }

    private static DomainResult ValidateCommon(
        DateOnly? employeeHireDate,
        DateOnly? latestExistingEffectiveFrom,
        decimal? minimumMonthlySalary,
        SalaryDecreeDto? salaryProfile)
    {
        if (salaryProfile is null)
            return DomainResult.Failure("اطلاعات پروفایل حقوق کارمند نمیتواند خالی باشد.");

        if (employeeHireDate is null)
            return DomainResult.Failure("تاریخ استخدام کارمند نمیتواند خالی باشد.");

        if (salaryProfile.EffectiveFrom is null)
            return DomainResult.Failure("تاریخ اعمال نمیتواند خالی باشد.");

        if (salaryProfile.EffectiveFrom < employeeHireDate)
            return DomainResult.Failure("تاریخ اعمال نباید قبل از تاریخ استخدام کارمند باشد.");

        if (latestExistingEffectiveFrom is not null &&
            salaryProfile.EffectiveFrom <= latestExistingEffectiveFrom)
            return DomainResult.Failure("تاریخ اعمال نباید قبل از پروفایل حقوق قبلی کارمند باشد.");

        if (minimumMonthlySalary is null)
            return DomainResult.Failure("حداقل حقوق ماهانه نمیتواند خالی باشد.");

        if (minimumMonthlySalary <= 0)
            return DomainResult.Failure("حداقل حقوق ماهانه باید بیشتر از صفر ریال باشد.");

        if (salaryProfile.BaseDailySalary is null)
            return DomainResult.Failure("حقوق پایه روزانه نمیتواند خالی باشد.");

        if (salaryProfile.BaseDailySalary <= 0)
            return DomainResult.Failure("حقوق پایه روزانه باید بیشتر از صفر ریال باشد.");

        if (salaryProfile.BaseDailySalary < minimumMonthlySalary)
            return DomainResult.Failure("حقوق پایه روزانه نمیتواند کمتر از حداقل حقوق ماهانه باشد.");

        var optionalAmountResult = ValidateOptionalAmount(salaryProfile.AttractionAllowance, "حق جذب");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        optionalAmountResult = ValidateOptionalAmount(salaryProfile.SupervisionAllowance, "حق سرپرستی");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        if (salaryProfile.ShiftType is null)
            return DomainResult.Failure("نوع شیفت نمیتواند خالی باشد.");

        if (salaryProfile.ContractType is null)
            return DomainResult.Failure("نوع قرارداد نمیتواند خالی باشد.");

        if (!Enum.IsDefined(typeof(ContractType), salaryProfile.ContractType.Value))
            return DomainResult.Failure("نوع قرارداد معتبر نیست.");

        optionalAmountResult = ValidateOptionalAmount(salaryProfile.TransportationAllowanceNet, "حق ایاب و ذهاب خالص");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        if (salaryProfile.MaritalStatus is null)
            return DomainResult.Failure("وضعیت تاهل نمیتواند خالی باشد.");

        if (!Enum.IsDefined(typeof(EmployeeMaritalStatus), salaryProfile.MaritalStatus.Value))
            return DomainResult.Failure("وضعیت تاهل معتبر نیست.");

        if (salaryProfile.ChildrenCount is null)
            return DomainResult.Failure("تعداد فرزندان نمیتواند خالی باشد.");

        if (salaryProfile.ChildrenCount < 0 || salaryProfile.ChildrenCount > 20)
            return DomainResult.Failure("تعداد فرزندان باید بین 0 تا 20 باشد.");

        if (salaryProfile.MaritalStatus == EmployeeMaritalStatus.Single && salaryProfile.ChildrenCount > 0)
            return DomainResult.Failure("برای کارمند مجرد، تعداد فرزندان باید صفر باشد.");

        if (salaryProfile.IsTaxSubject is null)
            return DomainResult.Failure("مشمول مالیات نمیتواند خالی باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateOptionalAmount(decimal? amount, string fieldName)
    {
        if (amount is null)
            return DomainResult.Success();

        if (amount <= 0)
            return DomainResult.Failure($"{fieldName} در صورت وارد شدن باید بیشتر از صفر ریال باشد.");

        return DomainResult.Success();
    }
}
