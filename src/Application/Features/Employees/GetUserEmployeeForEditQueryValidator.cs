namespace Application.Features.Employees;

public class GetUserEmployeeForEditQueryValidator : AbstractValidator<GetUserEmployeeForEditQuery>
{
    public GetUserEmployeeForEditQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEmpty().WithMessage("شناسه کارمند نمی‌تواند خالی باشد.");
    }
}
