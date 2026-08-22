using Core.Contracts;

namespace Application.Validations;

public class PaginationDtoValidator: AbstractValidator<PaginationDto>
{
    public PaginationDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("شماره صفحه باید بزرگتر از صفر باشد.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("تعداد آیتم‌ها در هر صفحه باید بزرگتر از صفر باشد.")
            .LessThanOrEqualTo(100).WithMessage("تعداد آیتم‌ها در هر صفحه نباید بیشتر از 100 باشد.");
    }
}