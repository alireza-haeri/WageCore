namespace Application.Features.SalaryDecrees;

public class GetSalaryDecreeForEditQueryValidator : AbstractValidator<GetSalaryDecreeForEditQuery>
{
    public GetSalaryDecreeForEditQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");

        RuleFor(x => x.SalaryDecreeId)
            .NotEmpty().WithMessage("شناسه پروفایل حقوق کارمند نمیتواند خالی باشد.");
    }
}
