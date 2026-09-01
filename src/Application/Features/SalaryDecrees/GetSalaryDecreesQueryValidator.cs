namespace Application.Features.SalaryDecrees;

public class GetSalaryDecreesQueryValidator : AbstractValidator<GetSalaryDecreesQuery>
{
    public GetSalaryDecreesQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.EmployeeId)
            .NotEqual(Guid.Empty).WithMessage("شناسه کارمند نمیتواند خالی باشد.")
            .When(x => x.EmployeeId.HasValue);

        RuleFor(x => x.WorkshopId)
            .NotEqual(Guid.Empty).WithMessage("شناسه کارگاه نمیتواند خالی باشد.")
            .When(x => x.WorkshopId.HasValue);

        RuleFor(x => x.DepartmentId)
            .NotEqual(Guid.Empty).WithMessage("شناسه بخش نمیتواند خالی باشد.")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("وضعیت پروفایل حقوق نامعتبر است.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("عبارت جستجو نباید بیشتر از 100 حرف باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationDtoValidator());
    }
}
