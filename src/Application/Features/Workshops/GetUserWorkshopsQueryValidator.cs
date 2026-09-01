using Application.Validations;

namespace Application.Features.Workshops;

public class GetUserWorkshopsQueryValidator : AbstractValidator<GetUserWorkshopsQuery>
{
    public GetUserWorkshopsQueryValidator()
    {
        RuleFor(x=>x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");
        
        RuleFor(x=>x.SearchName)
            .MaximumLength(200).WithMessage("نام کارگاه نباید بیشتر از 200 حرف باشد.")
            .When(x=>!string.IsNullOrWhiteSpace(x.SearchName));
        
        RuleFor(x=>x.Pagination)
            .SetValidator(new PaginationDtoValidator());
    }
}