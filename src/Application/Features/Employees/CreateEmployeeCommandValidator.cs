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

        RuleFor(x => x.Insurance)
            .NotNull().WithMessage("اطلاعات بیمه اجباری است.")
            .SetValidator(new EmployeeInsuranceValidator());
    }
}
