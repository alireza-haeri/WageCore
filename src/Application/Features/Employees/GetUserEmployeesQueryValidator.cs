namespace Application.Features.Employees;

public class GetUserEmployeesQueryValidator : AbstractValidator<GetUserEmployeesQuery>
{
    public GetUserEmployeesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.WorkshopId)
            .NotEqual(Guid.Empty).WithMessage("شناسه کارگاه نمیتواند خالی باشد.")
            .When(x => x.WorkshopId.HasValue);

        RuleFor(x => x.DepartmentId)
            .NotEqual(Guid.Empty).WithMessage("شناسه بخش نمیتواند خالی باشد.")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("وضعیت کارمند نامعتبر است.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("عبارت جستجو نباید بیشتر از 100 حرف باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationDtoValidator());
    }
}
