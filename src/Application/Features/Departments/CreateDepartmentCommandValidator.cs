namespace Application.Features.Departments;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("شناسه کارگاه نمیتواند خالی باشد.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام دپارتمان اجباری است.")
            .MinimumLength(2).WithMessage("نام دپارتمان نمیتواند کمتر از 2 کاراکتر باشد.")
            .MaximumLength(100).WithMessage("نام دپارتمان نمیتواند بیشتر از 100 کاراکتر باشد.")
            .WithName("نام دپارتمان");
    }
}
