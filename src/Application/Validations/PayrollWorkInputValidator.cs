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
            .SetValidator(new WorkTimeInputValidator("اضافه‌کاری"));

        RuleFor(x => x.NightShift)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات شیفت شب اجباری است.")
            .SetValidator(new WorkTimeInputValidator("شیفت شب"));

        RuleFor(x => x.FridayWork)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات کار جمعه اجباری است.")
            .SetValidator(new WorkTimeInputValidator("کار جمعه"));

        RuleFor(x => x.HolidayWork)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات تعطیل‌کاری اجباری است.")
            .SetValidator(new WorkTimeInputValidator("تعطیل‌کاری"));

        RuleFor(x => x.Leave)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("اطلاعات مرخصی اجباری است.")
            .SetValidator(new DayTimeInputValidator("مرخصی"));

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
            .SetValidator(new WorkTimeInputValidator("مأموریت"));

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
    private readonly string _workFieldName;

    public WorkTimeInputValidator(string workFieldName)
    {
        _workFieldName = workFieldName;

        RuleFor(x => x.Hours)
            .GreaterThanOrEqualTo(0)
            .WithMessage($"ساعات {_workFieldName} نمیتواند منفی باشد.");

        RuleFor(x => x.Minutes)
            .InclusiveBetween(0, 59)
            .WithMessage($"دقیقه {_workFieldName} باید بین 0 تا 59 باشد.");
    }
}

public class DayTimeInputValidator : AbstractValidator<DayTimeInput>
{
    private readonly string _workFieldName;

    public DayTimeInputValidator(string workFieldName)
    {
        _workFieldName = workFieldName;

        RuleFor(x => x.Days)
            .InclusiveBetween(0, 31)
            .WithMessage($"روزهای {_workFieldName} باید بین 0 تا 31 روز باشد.");

        RuleFor(x => x.Hours)
            .GreaterThanOrEqualTo(0)
            .WithMessage($"ساعات {_workFieldName} نمیتواند منفی باشد.");

        RuleFor(x => x.Minutes)
            .InclusiveBetween(0, 59)
            .WithMessage($"دقیقه {_workFieldName} باید بین 0 تا 59 باشد.");
    }
}
