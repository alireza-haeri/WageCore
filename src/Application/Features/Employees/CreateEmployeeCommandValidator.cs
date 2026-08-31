using Application.Validations;

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

        RuleFor(x => x.BankAccounts)
            .NotNull().WithMessage("اطلاعات حساب‌های بانکی اجباری است.")
            .NotEmpty().WithMessage("کارمند باید حداقل یک حساب بانکی داشته باشد.");

        RuleForEach(x => x.BankAccounts)
            .SetValidator(new EmployeeBankAccountValidator());
    }
}
