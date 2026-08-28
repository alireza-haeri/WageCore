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
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        PayrollRecordDto? payrollRecord)
    {
        var validationResult = Validate(
            payrollRecordId,
            employeeId,
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            payrollRecord);

        if (!validationResult.IsSuccess)
            return DomainResult<PayrollRecord>.Failure(validationResult.ErrorMessage!);

        return DomainResult<PayrollRecord>.Success(new PayrollRecord
        {
            Id = payrollRecordId,
            EmployeeId = employeeId,
            PeriodStart = payrollRecord!.PeriodStart!.Value,
            PeriodEnd = payrollRecord.PeriodEnd!.Value,
            WorkedDaysCount = payrollRecord.WorkedDaysCount!.Value,
            OvertimeHours = payrollRecord.OvertimeHours!.Value,
            NightShiftHours = payrollRecord.NightShiftHours!.Value,
            FridayWorkHours = payrollRecord.FridayWorkHours!.Value,
            LeaveDaysCount = payrollRecord.LeaveDaysCount!.Value,
            AbsenceDaysCount = payrollRecord.AbsenceDaysCount!.Value,
            MissionDaysCount = payrollRecord.MissionDaysCount!.Value,
            OvertimeAmount = payrollRecord.OvertimeAmount!.Value,
            NightShiftExtraAmount = payrollRecord.NightShiftExtraAmount!.Value,
            FridayWorkAllowance = payrollRecord.FridayWorkAllowance!.Value,
            CalculatedTaxAmount = payrollRecord.CalculatedTaxAmount!.Value,
            NetPayableAmount = payrollRecord.NetPayableAmount!.Value,
            Status = PayrollRecordStatus.Draft
        });
    }

    public static DomainResult<PayrollRecord> Create(
        Guid employeeId,
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        PayrollRecordDto? payrollRecord) =>
        Create(
            Guid.NewGuid(),
            employeeId,
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            payrollRecord);

    public DomainResult Update(
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        PayrollRecordDto? payrollRecord)
    {
        var canModifyResult = EnsureCanModify();
        if (!canModifyResult.IsSuccess)
            return canModifyResult;

        var validationResult = ValidateCommon(
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            payrollRecord);

        if (!validationResult.IsSuccess)
            return validationResult;

        PeriodStart = payrollRecord!.PeriodStart!.Value;
        PeriodEnd = payrollRecord.PeriodEnd!.Value;
        WorkedDaysCount = payrollRecord.WorkedDaysCount!.Value;
        OvertimeHours = payrollRecord.OvertimeHours!.Value;
        NightShiftHours = payrollRecord.NightShiftHours!.Value;
        FridayWorkHours = payrollRecord.FridayWorkHours!.Value;
        LeaveDaysCount = payrollRecord.LeaveDaysCount!.Value;
        AbsenceDaysCount = payrollRecord.AbsenceDaysCount!.Value;
        MissionDaysCount = payrollRecord.MissionDaysCount!.Value;
        OvertimeAmount = payrollRecord.OvertimeAmount!.Value;
        NightShiftExtraAmount = payrollRecord.NightShiftExtraAmount!.Value;
        FridayWorkAllowance = payrollRecord.FridayWorkAllowance!.Value;
        CalculatedTaxAmount = payrollRecord.CalculatedTaxAmount!.Value;
        NetPayableAmount = payrollRecord.NetPayableAmount!.Value;

        return DomainResult.Success();
    }

    public DomainResult MarkAsPaid()
    {
        var canModifyResult = EnsureCanModify();
        if (!canModifyResult.IsSuccess)
            return canModifyResult;

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
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        PayrollRecordDto? payrollRecord)
    {
        if (payrollRecordId == Guid.Empty)
            return DomainResult.Failure("شناسه فیش پرداختی نمیتواند خالی باشد.");

        if (employeeId == Guid.Empty)
            return DomainResult.Failure("شناسه کارمند نمیتواند خالی باشد.");

        return ValidateCommon(
            employeeIsTaxSubject,
            maxMonthlyOvertimeHours,
            maxFridayHours,
            payrollRecord);
    }

    private static DomainResult ValidateCommon(
        bool employeeIsTaxSubject,
        decimal? maxMonthlyOvertimeHours,
        decimal? maxFridayHours,
        PayrollRecordDto? payrollRecord)
    {
        if (payrollRecord is null)
            return DomainResult.Failure("اطلاعات فیش پرداختی نمیتواند خالی باشد.");

        var periodResult = ValidatePeriod(payrollRecord);
        if (!periodResult.IsSuccess)
            return periodResult;

        var daysCountResult = ValidateDaysCount(payrollRecord.WorkedDaysCount, "تعداد روزهای کارکرد");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateDaysCount(payrollRecord.LeaveDaysCount, "تعداد روزهای مرخصی");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateDaysCount(payrollRecord.AbsenceDaysCount, "تعداد روزهای غیبت");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        daysCountResult = ValidateDaysCount(payrollRecord.MissionDaysCount, "تعداد روزهای مأموریت");
        if (!daysCountResult.IsSuccess)
            return daysCountResult;

        var overtimeHoursResult = ValidateNonNegative(payrollRecord.OvertimeHours, "ساعات اضافه‌کاری");
        if (!overtimeHoursResult.IsSuccess)
            return overtimeHoursResult;

        var nightShiftHoursResult = ValidateNonNegative(payrollRecord.NightShiftHours, "ساعات شیفت شب");
        if (!nightShiftHoursResult.IsSuccess)
            return nightShiftHoursResult;

        var fridayWorkHoursResult = ValidateNonNegative(payrollRecord.FridayWorkHours, "ساعات کار جمعه");
        if (!fridayWorkHoursResult.IsSuccess)
            return fridayWorkHoursResult;

        if (maxMonthlyOvertimeHours is null)
            return DomainResult.Failure("حداکثر ساعات اضافه‌کاری ماهانه نمیتواند خالی باشد.");

        if (maxFridayHours is null)
            return DomainResult.Failure("حداکثر ساعات کار جمعه نمیتواند خالی باشد.");

        if (payrollRecord.OvertimeHours > maxMonthlyOvertimeHours)
            return DomainResult.Failure("ساعات اضافه‌کاری نباید بیشتر از حداکثر ساعات اضافه‌کاری ماهانه باشد.");

        if (payrollRecord.FridayWorkHours > maxFridayHours)
            return DomainResult.Failure("ساعات کار جمعه نباید بیشتر از حداکثر ساعات کار جمعه باشد.");

        var overtimeAmountResult = ValidateNonNegative(payrollRecord.OvertimeAmount, "مبلغ اضافه‌کاری");
        if (!overtimeAmountResult.IsSuccess)
            return overtimeAmountResult;

        var nightShiftExtraResult = ValidateNonNegative(payrollRecord.NightShiftExtraAmount, "فوق‌العاده شیفت شب");
        if (!nightShiftExtraResult.IsSuccess)
            return nightShiftExtraResult;

        var fridayWorkAllowanceResult = ValidateNonNegative(payrollRecord.FridayWorkAllowance, "حق کار جمعه");
        if (!fridayWorkAllowanceResult.IsSuccess)
            return fridayWorkAllowanceResult;

        var taxAmountResult = ValidateNonNegative(payrollRecord.CalculatedTaxAmount, "مالیات محاسبه شده");
        if (!taxAmountResult.IsSuccess)
            return taxAmountResult;

        if (employeeIsTaxSubject && payrollRecord.CalculatedTaxAmount == 0)
            return DomainResult.Failure("برای کارمند مشمول مالیات، مالیات محاسبه شده نمیتواند صفر باشد.");

        if (payrollRecord.NetPayableAmount is null)
            return DomainResult.Failure("مبلغ خالص قابل پرداخت نمیتواند خالی باشد.");

        return DomainResult.Success();
    }

    private static DomainResult ValidatePeriod(PayrollRecordDto payrollRecord)
    {
        if (payrollRecord.PeriodStart is null)
            return DomainResult.Failure("تاریخ شروع دوره نمیتواند خالی باشد.");

        if (payrollRecord.PeriodEnd is null)
            return DomainResult.Failure("تاریخ پایان دوره نمیتواند خالی باشد.");

        if (payrollRecord.PeriodEnd < payrollRecord.PeriodStart)
            return DomainResult.Failure("تاریخ پایان دوره نباید قبل از تاریخ شروع دوره باشد.");

        var periodLengthInDays = payrollRecord.PeriodEnd.Value.DayNumber -
                                 payrollRecord.PeriodStart.Value.DayNumber +
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

    private DomainResult EnsureCanModify() =>
        IsPaid
            ? DomainResult.Failure("فیش پرداختی پرداخت شده قابل ویرایش نیست.")
            : DomainResult.Success();
}
