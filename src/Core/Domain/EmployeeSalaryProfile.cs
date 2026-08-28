namespace Core.Domain;

public class EmployeeSalaryProfile
{
    public const string TableName = "EmployeeSalaryProfiles";

    public Guid Id { get; private init; }
    public Guid EmployeeId { get; private init; }
    public DateOnly EffectiveFrom { get; private set; }
    public decimal BaseMonthlySalary { get; private set; }
    public decimal? AttractionAllowance { get; private set; }
    public decimal? SupervisionAllowance { get; private set; }
    public SeniorityBaseApplicationMode SeniorityBaseApplicationMode { get; private set; }
    public SeniorityBaseCalculationMethod? SeniorityBaseCalculationMethod { get; private set; }
    public YearEndSeniorityMode YearEndSeniorityMode { get; private set; }
    public ShiftType ShiftType { get; private set; }
    public decimal? HousingAllowance { get; private set; }
    public decimal? FoodAllowance { get; private set; }
    public decimal? ChildAllowancePerChild { get; private set; }
    public decimal? TransportationAllowanceNet { get; private set; }
    public decimal? KaranehAmountNet { get; private set; }

    public static DomainResult<EmployeeSalaryProfile> Create(
        Guid salaryProfileId,
        Guid employeeId,
        DateOnly? employeeHireDate,
        DateOnly? latestExistingEffectiveFrom,
        decimal? minimumMonthlySalary,
        EmployeeSalaryProfileDto? salaryProfile)
    {
        var validationResult = Validate(
            salaryProfileId,
            employeeId,
            employeeHireDate,
            latestExistingEffectiveFrom,
            minimumMonthlySalary,
            salaryProfile);

        if (!validationResult.IsSuccess)
            return DomainResult<EmployeeSalaryProfile>.Failure(validationResult.ErrorMessage!);

        return DomainResult<EmployeeSalaryProfile>.Success(new EmployeeSalaryProfile
        {
            Id = salaryProfileId,
            EmployeeId = employeeId,
            EffectiveFrom = salaryProfile!.EffectiveFrom!.Value,
            BaseMonthlySalary = salaryProfile.BaseMonthlySalary!.Value,
            AttractionAllowance = salaryProfile.AttractionAllowance,
            SupervisionAllowance = salaryProfile.SupervisionAllowance,
            SeniorityBaseApplicationMode = salaryProfile.SeniorityBaseApplicationMode!.Value,
            SeniorityBaseCalculationMethod = salaryProfile.SeniorityBaseCalculationMethod,
            YearEndSeniorityMode = salaryProfile.YearEndSeniorityMode!.Value,
            ShiftType = salaryProfile.ShiftType!.Value,
            HousingAllowance = salaryProfile.HousingAllowance,
            FoodAllowance = salaryProfile.FoodAllowance,
            ChildAllowancePerChild = salaryProfile.ChildAllowancePerChild,
            TransportationAllowanceNet = salaryProfile.TransportationAllowanceNet,
            KaranehAmountNet = salaryProfile.KaranehAmountNet
        });
    }

    public static DomainResult<EmployeeSalaryProfile> Create(
        Guid employeeId,
        DateOnly? employeeHireDate,
        DateOnly? latestExistingEffectiveFrom,
        decimal? minimumMonthlySalary,
        EmployeeSalaryProfileDto? salaryProfile) =>
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
        EmployeeSalaryProfileDto? salaryProfile)
    {
        var validationResult = ValidateCommon(
            employeeHireDate,
            latestExistingEffectiveFrom,
            minimumMonthlySalary,
            salaryProfile);

        if (!validationResult.IsSuccess)
            return validationResult;

        EffectiveFrom = salaryProfile!.EffectiveFrom!.Value;
        BaseMonthlySalary = salaryProfile.BaseMonthlySalary!.Value;
        AttractionAllowance = salaryProfile.AttractionAllowance;
        SupervisionAllowance = salaryProfile.SupervisionAllowance;
        SeniorityBaseApplicationMode = salaryProfile.SeniorityBaseApplicationMode!.Value;
        SeniorityBaseCalculationMethod = salaryProfile.SeniorityBaseCalculationMethod;
        YearEndSeniorityMode = salaryProfile.YearEndSeniorityMode!.Value;
        ShiftType = salaryProfile.ShiftType!.Value;
        HousingAllowance = salaryProfile.HousingAllowance;
        FoodAllowance = salaryProfile.FoodAllowance;
        ChildAllowancePerChild = salaryProfile.ChildAllowancePerChild;
        TransportationAllowanceNet = salaryProfile.TransportationAllowanceNet;
        KaranehAmountNet = salaryProfile.KaranehAmountNet;

        return DomainResult.Success();
    }

    private static DomainResult Validate(
        Guid salaryProfileId,
        Guid employeeId,
        DateOnly? employeeHireDate,
        DateOnly? latestExistingEffectiveFrom,
        decimal? minimumMonthlySalary,
        EmployeeSalaryProfileDto? salaryProfile)
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
        EmployeeSalaryProfileDto? salaryProfile)
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

        if (salaryProfile.BaseMonthlySalary is null)
            return DomainResult.Failure("حقوق پایه ماهانه نمیتواند خالی باشد.");

        if (salaryProfile.BaseMonthlySalary <= 0)
            return DomainResult.Failure("حقوق پایه ماهانه باید بیشتر از صفر ریال باشد.");

        if (salaryProfile.BaseMonthlySalary < minimumMonthlySalary)
            return DomainResult.Failure("حقوق پایه ماهانه نمیتواند کمتر از حداقل حقوق ماهانه باشد.");

        var optionalAmountResult = ValidateOptionalAmount(salaryProfile.AttractionAllowance, "حق جذب");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        optionalAmountResult = ValidateOptionalAmount(salaryProfile.SupervisionAllowance, "حق سرپرستی");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        if (salaryProfile.SeniorityBaseApplicationMode is null)
            return DomainResult.Failure("نحوه اعمال پایه سنوات نمیتواند خالی باشد.");

        if (salaryProfile.SeniorityBaseApplicationMode == SeniorityBaseApplicationMode.Automatic &&
            salaryProfile.SeniorityBaseCalculationMethod is null)
            return DomainResult.Failure("روش محاسبه پایه سنوات در حالت خودکار الزامی است.");

        if (salaryProfile.SeniorityBaseApplicationMode == SeniorityBaseApplicationMode.Manual &&
            salaryProfile.SeniorityBaseCalculationMethod is not null)
            return DomainResult.Failure("روش محاسبه پایه سنوات در حالت دستی نباید پر شود.");

        if (salaryProfile.YearEndSeniorityMode is null)
            return DomainResult.Failure("نحوه محاسبه سنوات پایان سال نمیتواند خالی باشد.");

        if (salaryProfile.ShiftType is null)
            return DomainResult.Failure("نوع شیفت نمیتواند خالی باشد.");

        optionalAmountResult = ValidateOptionalAmount(salaryProfile.HousingAllowance, "حق مسکن");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        optionalAmountResult = ValidateOptionalAmount(salaryProfile.FoodAllowance, "حق بن خواربار");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        optionalAmountResult = ValidateOptionalAmount(salaryProfile.ChildAllowancePerChild, "حق اولاد به ازای هر فرزند");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        optionalAmountResult = ValidateOptionalAmount(salaryProfile.TransportationAllowanceNet, "حق ایاب و ذهاب خالص");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        optionalAmountResult = ValidateOptionalAmount(salaryProfile.KaranehAmountNet, "مبلغ خالص کارانه");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

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
