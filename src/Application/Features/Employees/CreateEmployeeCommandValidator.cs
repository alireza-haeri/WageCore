using Application.Validations;
using Core.Contracts.Employees;

namespace Application.Features.Employees;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("شناسه کارگاه نمیتواند خالی باشد.");

        RuleFor(x => x.Employee)
            .NotNull().WithMessage("اطلاعات کارمند اجباری است.")
            .SetValidator(new EmployeeInformationValidator());

        RuleFor(x => x.Insurance)
            .NotNull().WithMessage("اطلاعات بیمه اجباری است.")
            .SetValidator(new EmployeeInsuranceValidator());
    }

    private sealed class EmployeeInsuranceValidator : AbstractValidator<EmployeeInsuranceDto>
    {
        public EmployeeInsuranceValidator()
        {
            RuleFor(x => x.InsuranceNumber)
                .NotEmpty().WithMessage("شماره بیمه اجباری است.")
                .MaximumLength(20).WithMessage("شماره بیمه نمیتواند بیشتر از 20 کاراکتر باشد.");

            RuleFor(x => x.SocialSecurityContractRow)
                .MaximumLength(20).WithMessage("ردیف پیمان تامین اجتماعی نمیتواند بیشتر از 20 کاراکتر باشد.")
                .When(x => !string.IsNullOrWhiteSpace(x.SocialSecurityContractRow));

            RuleFor(x => x.PositionInInsuranceList)
                .NotEmpty().WithMessage("سمت در لیست بیمه اجباری است.")
                .MaximumLength(100).WithMessage("سمت در لیست بیمه نمیتواند بیشتر از 100 کاراکتر باشد.");

            RuleFor(x => x.InsuranceCalculationProfile)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("پروفایل محاسبه بیمه اجباری است.")
                .Must(x => x.HasValue && Enum.IsDefined(x.Value))
                .WithMessage("پروفایل محاسبه بیمه معتبر نیست.");
        }
    }
}
