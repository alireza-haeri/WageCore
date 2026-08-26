namespace Application.Features.Employees;

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.Employee)
            .NotNull().WithMessage("اطلاعات کارمند اجباری است.")
            .SetValidator(new EmployeeInformationValidator());

        RuleFor(x => x.Insurance)
            .NotNull().WithMessage("اطلاعات بیمه اجباری است.")
            .SetValidator(new EmployeeInsuranceValidator());

        RuleFor(x => x.BankAccounts)
            .NotNull().WithMessage("اطلاعات حساب‌های بانکی اجباری است.");

        RuleForEach(x => x.BankAccounts)
            .SetValidator(new EmployeeBankAccountValidator());
    }
}
