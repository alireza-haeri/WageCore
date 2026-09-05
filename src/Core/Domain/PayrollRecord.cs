namespace Core.Domain;

public class PayrollRecord
{
    public const string TableName = "PayrollRecords";
    public const int MaxPeriodLengthInDays = 31;
    public const int MaxDaysCount = 31;
    public const int MinStandardWorkingDaysCount = 28;
    public const int MaxStandardWorkingDaysCount = 31;

    public Guid Id { get; private init; }
    public Guid EmployeeId { get; private init; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public decimal WorkedDaysCount { get; private set; }
    public decimal OvertimeHours { get; private set; }
    public decimal NightShiftHours { get; private set; }
    public decimal FridayWorkHours { get; private set; }
    public decimal LeaveHours { get; private set; }
    public decimal AbsenceDaysCount { get; private set; }
    public decimal MissionDaysCount { get; private set; }
    public decimal MissionHours { get; private set; }
    public decimal HolidayWorkHours { get; private set; }
    public decimal? MissionAmountOverride { get; private set; }
    public int StandardWorkingDaysCount { get; private set; }
    public bool IsEsfandPeriod { get; private set; }
    public AnnualBonusType? AnnualBonusType { get; private set; }
    public decimal? PerformanceBonusAmount { get; private set; }
    public decimal? CashBenefitsAmount { get; private set; }
    public decimal OvertimeAmount { get; private set; }
    public decimal NightShiftExtraAmount { get; private set; }
    public decimal FridayWorkAllowance { get; private set; }
    public decimal BaseSalaryAmount { get; private set; }
    public decimal AttractionAllowanceAmount { get; private set; }
    public decimal SupervisionAllowanceAmount { get; private set; }
    public decimal HolidayWorkAmount { get; private set; }
    public decimal ChildAllowanceAmount { get; private set; }
    public decimal HousingAllowanceAmount { get; private set; }
    public decimal FoodAllowanceAmount { get; private set; }
    public decimal MarriageAllowanceAmount { get; private set; }
    public decimal ShiftWorkAmount { get; private set; }
    public decimal DailyMissionAmount { get; private set; }
    public decimal EndOfServiceAmount { get; private set; }
    public decimal? AnnualBonusAmount { get; private set; }
    public decimal CommutingAllowanceAmount { get; private set; }
    public decimal MaxMonthlyOvertimeHours { get; private set; }
    public decimal MaxFridayHours { get; private set; }
    public decimal MaxNightShiftHours { get; private set; }
    public decimal DailyWorkingHours { get; private set; }
    public decimal CalculatedTaxAmount { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal InsuranceAmount { get; private set; }
    public decimal TotalDeductionsAmount { get; private set; }
    public decimal NetPayableAmount { get; private set; }
    public PayrollRecordStatus Status { get; private set; }

    public bool IsPaid => Status == PayrollRecordStatus.Paid;

    public static DomainResult<PayrollRecord> Create(
        Guid payrollRecordId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        decimal? maxNightShiftHours,
        decimal? dailyWorkingHours,
        PayrollWorkInput workInput,
        PayrollRecordAmountsDto? payrollAmounts,
        PayrollCalculatedAmountsDto? calculatedAmounts)
    {
        var validationResult = Validate(
            payrollRecordId,
            employeeId,
            periodStart,
            periodEnd,
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            maxNightShiftHours,
            dailyWorkingHours,
            workInput,
            payrollAmounts,
            calculatedAmounts);

        if (!validationResult.IsSuccess)
            return DomainResult<PayrollRecord>.Failure(validationResult.ErrorMessage!);

        return DomainResult<PayrollRecord>.Success(new PayrollRecord
        {
            Id = payrollRecordId,
            EmployeeId = employeeId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            WorkedDaysCount = workInput.WorkedDaysCount,
            OvertimeHours = workInput.OvertimeHours,
            NightShiftHours = workInput.NightShiftHours,
            FridayWorkHours = workInput.FridayWorkHours,
            LeaveHours = workInput.LeaveHours,
            AbsenceDaysCount = workInput.AbsenceDaysCount,
            MissionDaysCount = workInput.MissionDaysCount,
            MissionHours = workInput.MissionHours,
            HolidayWorkHours = workInput.HolidayWorkHours,
            MissionAmountOverride = workInput.MissionAmountOverride,
            StandardWorkingDaysCount = workInput.StandardWorkingDaysCount,
            IsEsfandPeriod = workInput.IsEsfandPeriod,
            AnnualBonusType = workInput.AnnualBonusType,
            PerformanceBonusAmount = workInput.PerformanceBonusAmount,
            CashBenefitsAmount = workInput.CashBenefitsAmount,
            OvertimeAmount = calculatedAmounts!.OvertimeAmount,
            NightShiftExtraAmount = calculatedAmounts.NightShiftExtraAmount,
            FridayWorkAllowance = calculatedAmounts.FridayWorkAllowance,
            BaseSalaryAmount = calculatedAmounts.BaseSalaryAmount,
            AttractionAllowanceAmount = calculatedAmounts.AttractionAllowanceAmount,
            SupervisionAllowanceAmount = calculatedAmounts.SupervisionAllowanceAmount,
            HolidayWorkAmount = calculatedAmounts.HolidayWorkAmount,
            ChildAllowanceAmount = calculatedAmounts.ChildAllowanceAmount,
            HousingAllowanceAmount = calculatedAmounts.HousingAllowanceAmount,
            FoodAllowanceAmount = calculatedAmounts.FoodAllowanceAmount,
            MarriageAllowanceAmount = calculatedAmounts.MarriageAllowanceAmount,
            ShiftWorkAmount = calculatedAmounts.ShiftWorkAmount,
            DailyMissionAmount = calculatedAmounts.DailyMissionAmount,
            EndOfServiceAmount = calculatedAmounts.EndOfServiceAmount,
            AnnualBonusAmount = calculatedAmounts.AnnualBonusAmount,
            CommutingAllowanceAmount = calculatedAmounts.CommutingAllowanceAmount,
            MaxMonthlyOvertimeHours = maxMonthlyOvertimeHours!.Value,
            MaxFridayHours = maxFridayHours!.Value,
            MaxNightShiftHours = maxNightShiftHours!.Value,
            DailyWorkingHours = dailyWorkingHours!.Value,
            CalculatedTaxAmount = payrollAmounts!.CalculatedTaxAmount,
            GrossAmount = payrollAmounts.GrossAmount,
            InsuranceAmount = payrollAmounts.InsuranceAmount,
            TotalDeductionsAmount = payrollAmounts.TotalDeductionsAmount,
            NetPayableAmount = payrollAmounts.NetPayableAmount,
            Status = PayrollRecordStatus.Draft
        });
    }

    public static DomainResult<PayrollRecord> Create(
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        decimal? maxNightShiftHours,
        decimal? dailyWorkingHours,
        PayrollWorkInput workInput,
        PayrollRecordAmountsDto? payrollAmounts,
        PayrollCalculatedAmountsDto? calculatedAmounts) =>
        Create(
            Guid.NewGuid(),
            employeeId,
            periodStart,
            periodEnd,
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            maxNightShiftHours,
            dailyWorkingHours,
            workInput,
            payrollAmounts,
            calculatedAmounts);

    public DomainResult Update(
        DateOnly periodStart,
        DateOnly periodEnd,
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        decimal? maxNightShiftHours,
        decimal? dailyWorkingHours,
        PayrollWorkInput workInput,
        PayrollRecordAmountsDto? payrollAmounts,
        PayrollCalculatedAmountsDto? calculatedAmounts)
    {
        var canModifyResult = EnsureCanModify();
        if (!canModifyResult.IsSuccess)
            return canModifyResult;

        var validationResult = ValidateCommon(
            periodStart,
            periodEnd,
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            maxNightShiftHours,
            dailyWorkingHours,
            workInput,
            payrollAmounts,
            calculatedAmounts);

        if (!validationResult.IsSuccess)
            return validationResult;

        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        WorkedDaysCount = workInput.WorkedDaysCount;
        OvertimeHours = workInput.OvertimeHours;
        NightShiftHours = workInput.NightShiftHours;
        FridayWorkHours = workInput.FridayWorkHours;
        LeaveHours = workInput.LeaveHours;
        AbsenceDaysCount = workInput.AbsenceDaysCount;
        MissionDaysCount = workInput.MissionDaysCount;
        MissionHours = workInput.MissionHours;
        HolidayWorkHours = workInput.HolidayWorkHours;
        MissionAmountOverride = workInput.MissionAmountOverride;
        StandardWorkingDaysCount = workInput.StandardWorkingDaysCount;
        IsEsfandPeriod = workInput.IsEsfandPeriod;
        AnnualBonusType = workInput.AnnualBonusType;
        PerformanceBonusAmount = workInput.PerformanceBonusAmount;
        CashBenefitsAmount = workInput.CashBenefitsAmount;
        OvertimeAmount = calculatedAmounts!.OvertimeAmount;
        NightShiftExtraAmount = calculatedAmounts.NightShiftExtraAmount;
        FridayWorkAllowance = calculatedAmounts.FridayWorkAllowance;
        BaseSalaryAmount = calculatedAmounts.BaseSalaryAmount;
        AttractionAllowanceAmount = calculatedAmounts.AttractionAllowanceAmount;
        SupervisionAllowanceAmount = calculatedAmounts.SupervisionAllowanceAmount;
        HolidayWorkAmount = calculatedAmounts.HolidayWorkAmount;
        ChildAllowanceAmount = calculatedAmounts.ChildAllowanceAmount;
        HousingAllowanceAmount = calculatedAmounts.HousingAllowanceAmount;
        FoodAllowanceAmount = calculatedAmounts.FoodAllowanceAmount;
        MarriageAllowanceAmount = calculatedAmounts.MarriageAllowanceAmount;
        ShiftWorkAmount = calculatedAmounts.ShiftWorkAmount;
        DailyMissionAmount = calculatedAmounts.DailyMissionAmount;
        EndOfServiceAmount = calculatedAmounts.EndOfServiceAmount;
        AnnualBonusAmount = calculatedAmounts.AnnualBonusAmount;
        CommutingAllowanceAmount = calculatedAmounts.CommutingAllowanceAmount;
        MaxMonthlyOvertimeHours = maxMonthlyOvertimeHours!.Value;
        MaxFridayHours = maxFridayHours!.Value;
        MaxNightShiftHours = maxNightShiftHours!.Value;
        DailyWorkingHours = dailyWorkingHours!.Value;
        CalculatedTaxAmount = payrollAmounts!.CalculatedTaxAmount;
        GrossAmount = payrollAmounts.GrossAmount;
        InsuranceAmount = payrollAmounts.InsuranceAmount;
        TotalDeductionsAmount = payrollAmounts.TotalDeductionsAmount;
        NetPayableAmount = payrollAmounts.NetPayableAmount;

        return DomainResult.Success();
    }

    public DomainResult MarkAsPaid()
    {
        if (IsPaid)
            return DomainResult.Failure("فیش پرداختی قبلاً پرداخت شده است.");

        Status = PayrollRecordStatus.Paid;

        return DomainResult.Success();
    }

    public DomainResult EnsureCanDelete() =>
        IsPaid
            ? DomainResult.Failure("فیش پرداختی پرداخت شده قابل حذف نیست.")
            : DomainResult.Success();

    public static bool HasOverlap(
        DateOnly periodStart,
        DateOnly periodEnd,
        DateOnly otherPeriodStart,
        DateOnly otherPeriodEnd) =>
        periodStart <= otherPeriodEnd && otherPeriodStart <= periodEnd;

    public bool HasOverlap(DateOnly otherPeriodStart, DateOnly otherPeriodEnd) =>
        HasOverlap(PeriodStart, PeriodEnd, otherPeriodStart, otherPeriodEnd);

    private static DomainResult Validate(
        Guid payrollRecordId,
        Guid employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        decimal? maxNightShiftHours,
        decimal? dailyWorkingHours,
        PayrollWorkInput workInput,
        PayrollRecordAmountsDto? payrollAmounts,
        PayrollCalculatedAmountsDto? calculatedAmounts)
    {
        if (payrollRecordId == Guid.Empty)
            return DomainResult.Failure("شناسه فیش پرداختی نمیتواند خالی باشد.");

        if (employeeId == Guid.Empty)
            return DomainResult.Failure("شناسه کارمند نمیتواند خالی باشد.");

        return ValidateCommon(
            periodStart,
            periodEnd,
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            maxNightShiftHours,
            dailyWorkingHours,
            workInput,
            payrollAmounts,
            calculatedAmounts);
    }

    private static DomainResult ValidateCommon(
        DateOnly periodStart,
        DateOnly periodEnd,
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        decimal? maxNightShiftHours,
        decimal? dailyWorkingHours,
        PayrollWorkInput workInput,
        PayrollRecordAmountsDto? payrollAmounts,
        PayrollCalculatedAmountsDto? calculatedAmounts)
    {
        if (workInput is null)
            return DomainResult.Failure("اطلاعات فیش پرداختی نمیتواند خالی باشد.");

        if (payrollAmounts is null)
            return DomainResult.Failure("مبالغ فیش پرداختی نمیتواند خالی باشد.");

        if (calculatedAmounts is null)
            return DomainResult.Failure("مبالغ محاسبه شده فیش پرداختی نمیتواند خالی باشد.");

        var periodResult = ValidatePeriod(periodStart, periodEnd);
        if (!periodResult.IsSuccess)
            return periodResult;

        var attendanceResult = ValidateAttendance(
            workInput,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            maxNightShiftHours,
            dailyWorkingHours);
        if (!attendanceResult.IsSuccess)
            return attendanceResult;

        var annualBonusResult = ValidateAnnualBonus(workInput);
        if (!annualBonusResult.IsSuccess)
            return annualBonusResult;

        var amountsResult = ValidateAmounts(payrollAmounts, employeeIsTaxSubject);
        if (!amountsResult.IsSuccess)
            return amountsResult;

        return ValidateCalculatedAmounts(calculatedAmounts);
    }

    private static DomainResult ValidateAttendance(
        PayrollWorkInput workInput,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        decimal? maxNightShiftHours,
        decimal? dailyWorkingHours)
    {
        var daysCountResult = ValidateDaysCount(workInput.WorkedDaysCount, "تعداد روزهای کارکرد");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateNonNegative(workInput.LeaveHours, "ساعات مرخصی");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateDaysCount(workInput.AbsenceDaysCount, "تعداد روزهای غیبت");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateDaysCount(workInput.MissionDaysCount, "تعداد روزهای مأموریت");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        var standardDaysResult = ValidateStandardWorkingDaysCount(workInput.StandardWorkingDaysCount);
        if (!standardDaysResult.IsSuccess)
            return standardDaysResult;

        if (workInput.WorkedDaysCount > workInput.StandardWorkingDaysCount)
            return DomainResult.Failure("تعداد روزهای کارکرد نمیتواند بیشتر از روزهای کارکرد استاندارد باشد.");

        daysCountResult = ValidateNonNegative(workInput.MissionHours, "ساعات مأموریت");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateNonNegative(workInput.HolidayWorkHours, "ساعات تعطیل‌کاری");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        var optionalAmountResult = ValidateOptionalNonNegative(workInput.MissionAmountOverride, "مبلغ مأموریت");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        optionalAmountResult = ValidateOptionalNonNegative(workInput.PerformanceBonusAmount, "مبلغ کارانه");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        optionalAmountResult = ValidateOptionalNonNegative(workInput.CashBenefitsAmount, "مبلغ مزایای نقدی");
        if (!optionalAmountResult.IsSuccess)
            return optionalAmountResult;

        var overtimeHoursResult = ValidateNonNegative(workInput.OvertimeHours, "ساعات اضافه‌کاری");
        if (!overtimeHoursResult.IsSuccess)
            return overtimeHoursResult;

        var nightShiftHoursResult = ValidateNonNegative(workInput.NightShiftHours, "ساعات شیفت شب");
        if (!nightShiftHoursResult.IsSuccess)
            return nightShiftHoursResult;

        var fridayWorkHoursResult = ValidateNonNegative(workInput.FridayWorkHours, "ساعات کار جمعه");
        if (!fridayWorkHoursResult.IsSuccess)
            return fridayWorkHoursResult;

        if (maxMonthlyOvertimeHours is null)
            return DomainResult.Failure("حداکثر ساعات اضافه‌کاری ماهانه نمیتواند خالی باشد.");

        if (maxFridayHours is null)
            return DomainResult.Failure("حداکثر ساعات کار جمعه نمیتواند خالی باشد.");

        if (maxNightShiftHours is null)
            return DomainResult.Failure("حداکثر ساعات شب‌کاری ماهانه نمیتواند خالی باشد.");

        if (dailyWorkingHours is null)
            return DomainResult.Failure("ساعات کار روزانه نمیتواند خالی باشد.");

        if (dailyWorkingHours.Value <= 0)
            return DomainResult.Failure("ساعات کار روزانه باید بزرگ‌تر از صفر باشد.");

        if (workInput.OvertimeHours > maxMonthlyOvertimeHours)
            return DomainResult.Failure("ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.");

        if (workInput.NightShiftHours > maxNightShiftHours)
            return DomainResult.Failure("ساعات شب‌کاری نباید بیشتر از حداکثر ساعات شب‌کاری ماهانه باشد.");

        if (workInput.FridayWorkHours > maxFridayHours)
            return DomainResult.Failure("ساعات کار جمعه نباید بیشتر از حداکثر ساعات کار جمعه باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateAnnualBonus(PayrollWorkInput workInput)
    {
        if (!workInput.IsEsfandPeriod && workInput.AnnualBonusType is not null)
            return DomainResult.Failure("عیدی سالانه فقط در ماه اسفند قابل ثبت است.");

        if (workInput.IsEsfandPeriod && workInput.AnnualBonusType is null)
            return DomainResult.Failure("نوع عیدی سالانه نمیتواند خالی باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateAmounts(
        PayrollRecordAmountsDto payrollAmounts,
        bool employeeIsTaxSubject)
    {
        var taxAmountResult = ValidateNonNegativeAmount(payrollAmounts.CalculatedTaxAmount, "مالیات محاسبه شده");
        if (!taxAmountResult.IsSuccess)
            return taxAmountResult;

        if (employeeIsTaxSubject && payrollAmounts.CalculatedTaxAmount == 0)
            return DomainResult.Failure("برای کارمند مشمول مالیات، مالیات محاسبه شده نمیتواند صفر باشد.");

        var grossAmountResult = ValidateNonNegativeAmount(payrollAmounts.GrossAmount, "جمع حقوق و مزایا");
        if (!grossAmountResult.IsSuccess)
            return grossAmountResult;

        var insuranceAmountResult = ValidateNonNegativeAmount(payrollAmounts.InsuranceAmount, "بیمه ۷٪");
        if (!insuranceAmountResult.IsSuccess)
            return insuranceAmountResult;

        var totalDeductionsResult = ValidateNonNegativeAmount(payrollAmounts.TotalDeductionsAmount, "مالیات و کسورات");
        if (!totalDeductionsResult.IsSuccess)
            return totalDeductionsResult;

        var netPayableResult = ValidateNonNegativeAmount(payrollAmounts.NetPayableAmount, "حقوق نهایی");
        if (!netPayableResult.IsSuccess)
            return netPayableResult;

        return DomainResult.Success();
    }

    private static DomainResult ValidateCalculatedAmounts(PayrollCalculatedAmountsDto calculatedAmounts)
    {
        var result = ValidateNonNegativeAmount(calculatedAmounts.BaseSalaryAmount, "پایه حقوق ماهانه");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.AttractionAllowanceAmount, "حق جذب");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.SupervisionAllowanceAmount, "حق سرپرستی");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.NightShiftExtraAmount, "فوق‌العاده شیفت شب");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.HolidayWorkAmount, "مبلغ تعطیل‌کاری");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.ChildAllowanceAmount, "حق اولاد");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.HousingAllowanceAmount, "هزینه مسکن");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.FoodAllowanceAmount, "حق بن و خوار و بار");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.MarriageAllowanceAmount, "حق تأهل");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.OvertimeAmount, "مبلغ اضافه‌کاری");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.ShiftWorkAmount, "مبلغ نوبت‌کاری");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.DailyMissionAmount, "مبلغ مأموریت روزانه");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.FridayWorkAllowance, "حق کار جمعه");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.EndOfServiceAmount, "مبلغ سنوات پایان سال");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.AnnualBonusAmount ?? 0m, "مبلغ عیدی سالانه");
        if (!result.IsSuccess)
            return result;

        result = ValidateNonNegativeAmount(calculatedAmounts.CommutingAllowanceAmount, "مبلغ ایاب و ذهاب");
        if (!result.IsSuccess)
            return result;

        return DomainResult.Success();
    }

    private static DomainResult ValidatePeriod(DateOnly periodStart, DateOnly periodEnd)
    {
        if (periodEnd < periodStart)
            return DomainResult.Failure("تاریخ پایان دوره نباید قبل از تاریخ شروع دوره باشد.");

        var periodLengthInDays = periodEnd.DayNumber -
                                 periodStart.DayNumber +
                                 1;

        if (periodLengthInDays > MaxPeriodLengthInDays)
            return DomainResult.Failure($"بازه دوره فیش پرداختی نباید بیشتر از {MaxPeriodLengthInDays} روز باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateDaysCount(decimal daysCount, string fieldName)
    {
        if (daysCount < 0 || daysCount > MaxDaysCount)
            return DomainResult.Failure($"{fieldName} باید بین 0 تا {MaxDaysCount} روز باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateStandardWorkingDaysCount(int standardWorkingDaysCount)
    {
        if (standardWorkingDaysCount < MinStandardWorkingDaysCount ||
            standardWorkingDaysCount > MaxStandardWorkingDaysCount)
            return DomainResult.Failure(
                $"تعداد روزهای کارکرد استاندارد باید بین {MinStandardWorkingDaysCount} تا {MaxStandardWorkingDaysCount} روز باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateNonNegative(decimal value, string fieldName)
    {
        if (value < 0)
            return DomainResult.Failure($"{fieldName} نمیتواند منفی باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateOptionalNonNegative(decimal? value, string fieldName)
    {
        if (value is null)
            return DomainResult.Success();

        if (value < 0)
            return DomainResult.Failure($"{fieldName} نمیتواند منفی باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateNonNegativeAmount(decimal value, string fieldName)
    {
        if (value < 0)
            return DomainResult.Failure($"{fieldName} نمیتواند منفی باشد.");

        return DomainResult.Success();
    }

    public DomainResult EnsureCanModify() =>
        IsPaid
            ? DomainResult.Failure("فیش پرداختی پرداخت شده قابل ویرایش نیست.")
            : DomainResult.Success();
}