namespace Application.Features.Departments;

public class GetDepartmentForEditQueryValidator : AbstractValidator<GetDepartmentForEditQuery>
{
    public GetDepartmentForEditQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("شناسه بخش نمی‌تواند خالی باشد.");
    }
}
