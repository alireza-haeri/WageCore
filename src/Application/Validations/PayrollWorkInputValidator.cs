namespace Application.Validations;

public class UserWorkInputValidator : AbstractValidator<UserWorkInputDto>
{
    public UserWorkInputValidator()
    {
        RuleFor(x => x.WorkedDaysCount)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 31)
            .WithMessage("تعداد روزهای کارکرد باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.LeaveHours)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ساعات مرخصی نمیتواند منفی باشد.");

        RuleFor(x => x.AbsenceDaysCount)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 31)
            .WithMessage("تعداد روزهای غیبت باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.MissionDaysCount)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 31)
            .WithMessage("تعداد روزهای مأموریت باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.MissionHours)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ساعات مأموریت نمیتواند منفی باشد.");

        RuleFor(x => x.HolidayWorkHours)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ساعات تعطیل‌کاری نمیتواند منفی باشد.");

        RuleFor(x => x.OvertimeHours)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ساعات اضافه‌کاری نمیتواند منفی باشد.");

        RuleFor(x => x.NightShiftHours)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ساعات شیفت شب نمیتواند منفی باشد.");

        RuleFor(x => x.FridayWorkHours)
            .Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ساعات کار جمعه نمیتواند منفی باشد.");

        RuleFor(x => x.MissionAmountOverride)
            .GreaterThanOrEqualTo(0)
            .WithMessage("مبلغ مأموریت نمیتواند منفی باشد.");

        RuleFor(x => x.PerformanceBonusAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("مبلغ کارانه نمیتواند منفی باشد.");

        RuleFor(x => x.CashBenefitsAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("مبلغ مزایای نقدی نمیتواند منفی باشد.");
    }
}