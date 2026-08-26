namespace Application.Features.Employees;

public class RehireEmployeeCommandValidator : AbstractValidator<RehireEmployeeCommand>
{
    public RehireEmployeeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمیتواند خالی باشد.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("شناسه بخش نمیتواند خالی باشد.");

        RuleFor(x => x.HireDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("تاریخ استخدام اجباری است.")
            .Must(x => x.HasValue && x.Value <= DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("تاریخ استخدام نباید برای آینده باشد.");
    }
}
