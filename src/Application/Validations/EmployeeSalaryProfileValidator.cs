using Core.Contracts.Employees;

namespace Application.Validations;

public class EmployeeSalaryProfileValidator : AbstractValidator<EmployeeSalaryProfileDto>
{
    public EmployeeSalaryProfileValidator()
    {
        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("تاریخ اعمال اجباری است.");

        RuleFor(x => x.BaseMonthlySalary)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("حقوق پایه ماهانه اجباری است.")
            .GreaterThan(0).WithMessage("حقوق پایه ماهانه باید بیشتر از صفر ریال باشد.");

        RuleFor(x => x.AttractionAllowance)
            .GreaterThan(0).WithMessage("حق جذب در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.AttractionAllowance.HasValue);

        RuleFor(x => x.SupervisionAllowance)
            .GreaterThan(0).WithMessage("حق سرپرستی در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.SupervisionAllowance.HasValue);

        RuleFor(x => x.SeniorityBaseApplicationMode)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("نحوه اعمال پایه سنوات اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("نحوه اعمال پایه سنوات معتبر نیست.");

        RuleFor(x => x.SeniorityBaseCalculationMethod)
            .NotNull().WithMessage("روش محاسبه پایه سنوات در حالت خودکار الزامی است.")
            .When(x => x.SeniorityBaseApplicationMode == SeniorityBaseApplicationMode.Automatic);

        RuleFor(x => x.SeniorityBaseCalculationMethod)
            .Null().WithMessage("روش محاسبه پایه سنوات در حالت دستی نباید پر شود.")
            .When(x => x.SeniorityBaseApplicationMode == SeniorityBaseApplicationMode.Manual);

        RuleFor(x => x.SeniorityBaseCalculationMethod)
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("روش محاسبه پایه سنوات معتبر نیست.")
            .When(x => x.SeniorityBaseCalculationMethod.HasValue);

        RuleFor(x => x.YearEndSeniorityMode)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("نحوه محاسبه سنوات پایان سال اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("نحوه محاسبه سنوات پایان سال معتبر نیست.");

        RuleFor(x => x.ShiftType)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("نوع شیفت اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("نوع شیفت معتبر نیست.");

        RuleFor(x => x.HousingAllowance)
            .GreaterThan(0).WithMessage("حق مسکن در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.HousingAllowance.HasValue);

        RuleFor(x => x.FoodAllowance)
            .GreaterThan(0).WithMessage("حق بن خواربار در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.FoodAllowance.HasValue);

        RuleFor(x => x.ChildAllowancePerChild)
            .GreaterThan(0).WithMessage("حق اولاد به ازای هر فرزند در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.ChildAllowancePerChild.HasValue);

        RuleFor(x => x.TransportationAllowanceNet)
            .GreaterThan(0).WithMessage("حق ایاب و ذهاب خالص در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.TransportationAllowanceNet.HasValue);

        RuleFor(x => x.KaranehAmountNet)
            .GreaterThan(0).WithMessage("مبلغ خالص کارانه در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.KaranehAmountNet.HasValue);
    }
}
