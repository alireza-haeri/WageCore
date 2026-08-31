using Core.Contracts.Employees;

namespace Application.Validations;

public class EmployeeInsuranceValidator : AbstractValidator<EmployeeInsuranceDto>
{
    public EmployeeInsuranceValidator()
    {
        RuleFor(x => x.InsuranceNumber)
            .NotEmpty().WithMessage("شماره بیمه اجباری است.")
            .MaximumLength(20).WithMessage("شماره بیمه نمیتواند بیشتر از 20 کاراکتر باشد.");

        RuleFor(x => x.PositionInInsuranceList)
            .NotEmpty().WithMessage("سمت در لیست بیمه اجباری است.")
            .MaximumLength(100).WithMessage("سمت در لیست بیمه نمیتواند بیشتر از 100 کاراکتر باشد.");

        RuleFor(x => x.IsSubjectTo4PercentInsurance)
            .NotNull().WithMessage("مشمول بیمه ۴ درصد اجباری است.");

        RuleFor(x => x.InsuranceCalculationProfile)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("پروفایل محاسبه بیمه اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("پروفایل محاسبه بیمه معتبر نیست.");
    }
}
