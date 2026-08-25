using Application.Validations;

namespace Application.Features.Departments;

public class GetUserDepartmentsQueryValidator : AbstractValidator<GetUserDepartmentsQuery>
{
    public GetUserDepartmentsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.WorkshopId)
            .NotEqual(Guid.Empty).WithMessage("شناسه کارگاه نمیتواند خالی باشد.")
            .When(x => x.WorkshopId.HasValue);

        RuleFor(x => x.SearchName)
            .MaximumLength(100).WithMessage("نام دپارتمان نباید بیشتر از 100 حرف باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.SearchName));

        RuleFor(x => x.Pagination)
            .SetValidator(new PaginationDtoValidator());
    }
}
