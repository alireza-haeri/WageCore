namespace Application.Features.PayrollRecords.Query.GetPayrollRecords;

public class GetPayrollRecordsQueryValidator : AbstractValidator<GetPayrollRecordsQuery>
{
    public GetPayrollRecordsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.WorkshopId)
            .NotEqual(Guid.Empty).WithMessage("شناسه کارگاه نمیتواند خالی باشد.")
            .When(x => x.WorkshopId.HasValue);

        RuleFor(x => x.DepartmentId)
            .NotEqual(Guid.Empty).WithMessage("شناسه بخش نمیتواند خالی باشد.")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.Search)
            .MaximumLength(100).WithMessage("عبارت جستجو نباید بیشتر از 100 حرف باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.PersianYear)
            .GreaterThan(0).WithMessage("سال شمسی باید بزرگتر از صفر باشد.")
            .When(x => x.PersianYear.HasValue);

        RuleFor(x => x.PersianMonth)
            .InclusiveBetween(1, 12).WithMessage("ماه شمسی باید بین 1 تا 12 باشد.")
            .When(x => x.PersianMonth.HasValue);

        RuleFor(x => x.PersianMonth)
            .Must((x, persianMonth) => x.PersianYear.HasValue)
            .WithMessage("ماه شمسی بدون سال شمسی قابل فیلتر نیست.")
            .When(x => x.PersianMonth.HasValue);

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationDtoValidator());
    }
}
