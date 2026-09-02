using Core.Contracts.PayrollRecords;

namespace Application.Validations;

public class PayrollWorkInputValidator : AbstractValidator<PayrollWorkInputDto>
{
    public PayrollWorkInputValidator()
    {
        RuleFor(x => x.WorkedDaysCount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("تعداد روزهای کارکرد اجباری است.")
            .Must(x => x.HasValue && x.Value >= 0 && x.Value <= 31)
            .WithMessage("تعداد روزهای کارکرد باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.LeaveHours)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("ساعات مرخصی اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("ساعات مرخصی نمیتواند منفی باشد.");

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

        RuleFor(x => x.MissionHours)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("ساعات مأموریت اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("ساعات مأموریت نمیتواند منفی باشد.");

        RuleFor(x => x.HolidayWorkHours)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("ساعات تعطیل‌کاری اجباری است.")
            .GreaterThanOrEqualTo(0).WithMessage("ساعات تعطیل‌کاری نمیتواند منفی باشد.");

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

        RuleFor(x => x.MissionAmountOverride)
            .Must(x => x is null || x.Value >= 0)
            .WithMessage("مبلغ مأموریت نمیتواند منفی باشد.");

        RuleFor(x => x.StandardWorkingDaysCount)
            .Must(x => x is null || (x.Value >= 28 && x.Value <= 31))
            .WithMessage("تعداد روزهای کارکرد استاندارد باید بین 28 تا 31 روز باشد.");

        RuleFor(x => x.PerformanceBonusAmount)
            .Must(x => x is null || x.Value >= 0)
            .WithMessage("مبلغ کارانه نمیتواند منفی باشد.");

        RuleFor(x => x.CashBenefitsAmount)
            .Must(x => x is null || x.Value >= 0)
            .WithMessage("مبلغ مزایای نقدی نمیتواند منفی باشد.");
    }
}
