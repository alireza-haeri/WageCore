namespace Application.Features.EmployeeSalaryProfiles;

public class CreateEmployeeSalaryProfileCommandValidator : AbstractValidator<CreateEmployeeSalaryProfileCommand>
{
    public CreateEmployeeSalaryProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.SalaryProfile)
            .NotNull().WithMessage("اطلاعات پروفایل حقوق کارمند اجباری است.")
            .SetValidator(new EmployeeSalaryProfileValidator());
    }
}
