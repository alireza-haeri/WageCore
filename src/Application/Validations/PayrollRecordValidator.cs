using Core.Contracts.PayrollRecords;

namespace Application.Validations;

public class PayrollRecordValidator : AbstractValidator<PayrollRecordDto>
{
    public PayrollRecordValidator()
    {
        RuleFor(x => x.PeriodStart)
            .NotEmpty().WithMessage("تاریخ شروع دوره اجباری است.");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty().WithMessage("تاریخ پایان دوره اجباری است.")
            .Must((payrollRecord, periodEnd) => periodEnd >= payrollRecord.PeriodStart)
            .WithMessage("تاریخ پایان دوره نباید قبل از تاریخ شروع دوره باشد.")
            .When(x => x.PeriodStart is not null && x.PeriodEnd is not null);

        RuleFor(x => x.WorkedDaysCount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("تعداد روزهای کارکرد اجباری است.")
            .Must(x => x.HasValue && x.Value >= 0 && x.Value <= 31)
            .WithMessage("تعداد روزهای کارکرد باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.LeaveDaysCount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("تعداد روزهای مرخصی اجباری است.")
            .Must(x => x.HasValue && x.Value >= 0 && x.Value <= 31)
            .WithMessage("تعداد روزهای مرخصی باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.AbsenceDaysCount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("تعداد روزهای غیبت اجباری است.")
            .Must(x => x.HasValue && x.Value >= 0 && x.Value <= 31)
            .WithMessage("تعداد روزهای غیبت باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.MissionDaysCount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("تعداد روزهای مأموریت اجباری است.")
            .Must(x => x.HasValue && x.Value >= 0 && x.Value <= 31)
            .WithMessage("تعداد روزهای مأموریت باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.OvertimeHours)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("ساعات اضافه‌کاری اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("ساعات اضافه‌کاری نمیتواند منفی باشد.");

        RuleFor(x => x.NightShiftHours)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("ساعات شیفت شب اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("ساعات شیفت شب نمیتواند منفی باشد.");

        RuleFor(x => x.FridayWorkHours)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("ساعات کار جمعه اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("ساعات کار جمعه نمیتواند منفی باشد.");

        RuleFor(x => x.OvertimeAmount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("مبلغ اضافه‌کاری اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("مبلغ اضافه‌کاری نمیتواند منفی باشد.");

        RuleFor(x => x.NightShiftExtraAmount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("فوق‌العاده شیفت شب اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("فوق‌العاده شیفت شب نمیتواند منفی باشد.");

        RuleFor(x => x.FridayWorkAllowance)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("حق کار جمعه اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("حق کار جمعه نمیتواند منفی باشد.");

        RuleFor(x => x.CalculatedTaxAmount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("مالیات محاسبه شده اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("مالیات محاسبه شده نمیتواند منفی باشد.");

        RuleFor(x => x.NetPayableAmount)
            .NotNull().WithMessage("مبلغ خالص قابل پرداخت اجباری است.");
    }
}
