using Core.Contracts.Employees;

namespace Application.Validations;

public class SalaryDecreeValidator : AbstractValidator<SalaryDecreeDto>
{
    public SalaryDecreeValidator()
    {
        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("تاریخ اعمال اجباری است.");

        RuleFor(x => x.BaseDailySalary)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("حقوق پایه روزانه اجباری است.")
            .GreaterThan(0).WithMessage("حقوق پایه روزانه باید بیشتر از صفر ریال باشد.");

        RuleFor(x => x.AttractionAllowance)
            .GreaterThan(0).WithMessage("حق جذب در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.AttractionAllowance.HasValue);

        RuleFor(x => x.SupervisionAllowance)
            .GreaterThan(0).WithMessage("حق سرپرستی در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.SupervisionAllowance.HasValue);

        RuleFor(x => x.ShiftType)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("نوع شیفت اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("نوع شیفت معتبر نیست.");

        RuleFor(x => x.ContractType)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("نوع قرارداد اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("نوع قرارداد معتبر نیست.");

        RuleFor(x => x.HousingAllowance)
            .GreaterThan(0).WithMessage("حق مسکن در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.HousingAllowance.HasValue);

        RuleFor(x => x.FoodAllowance)
            .GreaterThan(0).WithMessage("حق بن خواربار در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.FoodAllowance.HasValue);

        RuleFor(x => x.TransportationAllowanceNet)
            .GreaterThan(0).WithMessage("حق ایاب و ذهاب خالص در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.TransportationAllowanceNet.HasValue);

        RuleFor(x => x.KaranehAmountNet)
            .GreaterThan(0).WithMessage("مبلغ خالص کارانه در صورت وارد شدن باید بیشتر از صفر ریال باشد.")
            .When(x => x.KaranehAmountNet.HasValue);

        RuleFor(x => x.MaritalStatus)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("وضعیت تاهل اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("وضعیت تاهل معتبر نیست.");

        RuleFor(x => x.ChildrenCount)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("تعداد فرزندان اجباری است.")
            .Must(x => x.HasValue && x.Value >= 0 && x.Value <= 20)
            .WithMessage("تعداد فرزندان باید بین 0 تا 20 باشد.");

        RuleFor(x => x.ChildrenCount)
            .Equal(0)
            .WithMessage("برای کارمند مجرد، تعداد فرزندان باید صفر باشد.")
            .When(x => x.MaritalStatus == EmployeeMaritalStatus.Single && x.ChildrenCount.HasValue);

        RuleFor(x => x.IsTaxSubject)
            .NotNull().WithMessage("مشمول مالیات اجباری است.");

        RuleFor(x => x.Insurance)
            .NotNull().WithMessage("اطلاعات بیمه اجباری است.")
            .SetValidator(new EmployeeInsuranceValidator());
    }
}
