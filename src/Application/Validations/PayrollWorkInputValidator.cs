namespace Application.Validations;

public class UserWorkInputValidator : AbstractValidator<UserWorkInputDto>
{
    public UserWorkInputValidator()
    {
        RuleFor(x => x.WorkedDaysCount)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 31)
            .WithMessage("تعداد روزهای کارکرد باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.Overtime)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات اضافه‌کاری اجباری است.")
            .SetValidator(new WorkTimeInputValidator());

        RuleFor(x => x.NightShift)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات شیفت شب اجباری است.")
            .SetValidator(new WorkTimeInputValidator());

        RuleFor(x => x.FridayWork)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات کار جمعه اجباری است.")
            .SetValidator(new WorkTimeInputValidator());

        RuleFor(x => x.HolidayWork)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات تعطیل‌کاری اجباری است.")
            .SetValidator(new WorkTimeInputValidator());

        RuleFor(x => x.Leave)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات مرخصی اجباری است.")
            .SetValidator(new DayTimeInputValidator());

        RuleFor(x => x.AbsenceDaysCount)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 31)
            .WithMessage("تعداد روزهای غیبت باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.MissionDays)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0, 31)
            .WithMessage("تعداد روزهای مأموریت باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.MissionHours)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات ساعات مأموریت اجباری است.")
            .SetValidator(new WorkTimeInputValidator());

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

public class WorkTimeInputValidator : AbstractValidator<WorkTimeInput>
{
    public WorkTimeInputValidator()
    {
        RuleFor(x => x.Hours)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ساعت نمی‌تواند منفی باشد.");

        RuleFor(x => x.Minutes)
            .InclusiveBetween(0, 59)
            .WithMessage("دقیقه باید بین 0 تا 59 باشد.");
    }
}

public class DayTimeInputValidator : AbstractValidator<DayTimeInput>
{
    public DayTimeInputValidator()
    {
        RuleFor(x => x.Days)
            .InclusiveBetween(0, 31)
            .WithMessage("تعداد روز باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.Hours)
            .GreaterThanOrEqualTo(0)
            .WithMessage("ساعت نمی‌تواند منفی باشد.");

        RuleFor(x => x.Minutes)
            .InclusiveBetween(0, 59)
            .WithMessage("دقیقه باید بین 0 تا 59 باشد.");
    }
}
