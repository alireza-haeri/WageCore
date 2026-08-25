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
    }
}
