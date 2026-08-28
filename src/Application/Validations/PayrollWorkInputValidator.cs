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
    }
}
