namespace Core.Domain;

public class PayrollRecord
{
    public const string TableName = "PayrollRecords";
    public const int MaxPeriodLengthInDays = 31;
    public const int MaxDaysCount = 31;

    public Guid Id { get; private init; }
    public Guid EmployeeId { get; private init; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public decimal WorkedDaysCount { get; private set; }
    public decimal OvertimeHours { get; private set; }
    public decimal NightShiftHours { get; private set; }
    public decimal FridayWorkHours { get; private set; }
    public decimal LeaveDaysCount { get; private set; }
    public decimal AbsenceDaysCount { get; private set; }
    public decimal MissionDaysCount { get; private set; }
    public decimal OvertimeAmount { get; private set; }
    public decimal NightShiftExtraAmount { get; private set; }
    public decimal FridayWorkAllowance { get; private set; }
    public decimal CalculatedTaxAmount { get; private set; }
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
        PayrollWorkInputDto? workInput,
        PayrollRecordAmountsDto? payrollAmounts)
    {
        var validationResult = Validate(
            payrollRecordId,
            employeeId,
            periodStart,
            periodEnd,
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            workInput,
            payrollAmounts);

        if (!validationResult.IsSuccess)
            return DomainResult<PayrollRecord>.Failure(validationResult.ErrorMessage!);

        return DomainResult<PayrollRecord>.Success(new PayrollRecord
        {
            Id = payrollRecordId,
            EmployeeId = employeeId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            WorkedDaysCount = workInput.WorkedDaysCount!.Value,
            OvertimeHours = workInput.OvertimeHours!.Value,
            NightShiftHours = workInput.NightShiftHours!.Value,
            FridayWorkHours = workInput.FridayWorkHours!.Value,
            LeaveDaysCount = workInput.LeaveDaysCount!.Value,
            AbsenceDaysCount = workInput.AbsenceDaysCount!.Value,
            MissionDaysCount = workInput.MissionDaysCount!.Value,
            OvertimeAmount = payrollAmounts!.OvertimeAmount,
            NightShiftExtraAmount = payrollAmounts.NightShiftExtraAmount,
            FridayWorkAllowance = payrollAmounts.FridayWorkAllowance,
            CalculatedTaxAmount = payrollAmounts.CalculatedTaxAmount,
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
        PayrollWorkInputDto? workInput,
        PayrollRecordAmountsDto? payrollAmounts) =>
        Create(
            Guid.NewGuid(),
            employeeId,
            periodStart,
            periodEnd,
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            workInput,
            payrollAmounts);

    public DomainResult Update(
        DateOnly periodStart,
        DateOnly periodEnd,
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        PayrollWorkInputDto? workInput,
        PayrollRecordAmountsDto? payrollAmounts)
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
            workInput,
            payrollAmounts);

        if (!validationResult.IsSuccess)
            return validationResult;

        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        WorkedDaysCount = workInput.WorkedDaysCount!.Value;
        OvertimeHours = workInput.OvertimeHours!.Value;
        NightShiftHours = workInput.NightShiftHours!.Value;
        FridayWorkHours = workInput.FridayWorkHours!.Value;
        LeaveDaysCount = workInput.LeaveDaysCount!.Value;
        AbsenceDaysCount = workInput.AbsenceDaysCount!.Value;
        MissionDaysCount = workInput.MissionDaysCount!.Value;
        OvertimeAmount = payrollAmounts!.OvertimeAmount;
        NightShiftExtraAmount = payrollAmounts.NightShiftExtraAmount;
        FridayWorkAllowance = payrollAmounts.FridayWorkAllowance;
        CalculatedTaxAmount = payrollAmounts.CalculatedTaxAmount;
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
        PayrollWorkInputDto? workInput,
        PayrollRecordAmountsDto? payrollAmounts)
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
            workInput,
            payrollAmounts);
    }

    private static DomainResult ValidateCommon(
        DateOnly periodStart,
        DateOnly periodEnd,
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        PayrollWorkInputDto? workInput,
        PayrollRecordAmountsDto? payrollAmounts)
    {
        if (workInput is null)
            return DomainResult.Failure("اطلاعات فیش پرداختی نمیتواند خالی باشد.");

        if (payrollAmounts is null)
            return DomainResult.Failure("مبالغ فیش پرداختی نمیتواند خالی باشد.");

        var periodResult = ValidatePeriod(periodStart, periodEnd);
        if (!periodResult.IsSuccess)
            return periodResult;

        var attendanceResult = ValidateAttendance(workInput, maxMonthlyOvertimeHours, maxFridayHours);
        if (!attendanceResult.IsSuccess)
            return attendanceResult;

        return ValidateAmounts(payrollAmounts, employeeIsTaxSubject);
    }

    private static DomainResult ValidateAttendance(
        PayrollWorkInputDto workInput,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours)
    {
        var daysCountResult = ValidateDaysCount(workInput.WorkedDaysCount, "تعداد روزهای کارکرد");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateDaysCount(workInput.LeaveDaysCount, "تعداد روزهای مرخصی");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateDaysCount(workInput.AbsenceDaysCount, "تعداد روزهای غیبت");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateDaysCount(workInput.MissionDaysCount, "تعداد روزهای مأموریت");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

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

        if (workInput.OvertimeHours > maxMonthlyOvertimeHours)
            return DomainResult.Failure("ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.");

        if (workInput.FridayWorkHours > maxFridayHours)
            return DomainResult.Failure("ساعات کار جمعه نباید بیشتر از حداکثر ساعات کار جمعه باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateAmounts(
        PayrollRecordAmountsDto payrollAmounts,
        bool employeeIsTaxSubject)
    {
        var overtimeAmountResult = ValidateNonNegative(payrollAmounts.OvertimeAmount, "مبلغ اضافه‌کاری");
        if (!overtimeAmountResult.IsSuccess)
            return overtimeAmountResult;

        var nightShiftExtraResult = ValidateNonNegative(payrollAmounts.NightShiftExtraAmount, "فوق‌العاده شیفت شب");
        if (!nightShiftExtraResult.IsSuccess)
            return nightShiftExtraResult;

        var fridayWorkAllowanceResult = ValidateNonNegative(payrollAmounts.FridayWorkAllowance, "حق کار جمعه");
        if (!fridayWorkAllowanceResult.IsSuccess)
            return fridayWorkAllowanceResult;

        var taxAmountResult = ValidateNonNegative(payrollAmounts.CalculatedTaxAmount, "مالیات محاسبه شده");
        if (!taxAmountResult.IsSuccess)
            return taxAmountResult;

        if (employeeIsTaxSubject && payrollAmounts.CalculatedTaxAmount == 0)
            return DomainResult.Failure("برای کارمند مشمول مالیات، مالیات محاسبه شده نمیتواند صفر باشد.");

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

    private static DomainResult ValidateDaysCount(decimal? daysCount, string fieldName)
    {
        if (daysCount is null)
            return DomainResult.Failure($"{fieldName} نمیتواند خالی باشد.");

        if (daysCount < 0 || daysCount > MaxDaysCount)
            return DomainResult.Failure($"{fieldName} باید بین 0 تا {MaxDaysCount} روز باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidateNonNegative(decimal? value, string fieldName)
    {
        if (value is null)
            return DomainResult.Failure($"{fieldName} نمیتواند خالی باشد.");

        if (value < 0)
            return DomainResult.Failure($"{fieldName} نمیتواند منفی باشد.");

        return DomainResult.Success();
    }

    public DomainResult EnsureCanModify() =>
        IsPaid
            ? DomainResult.Failure("فیش پرداختی پرداخت شده قابل ویرایش نیست.")
            : DomainResult.Success();
}
