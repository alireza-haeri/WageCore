namespace Application.Features.Workshops;

public class GetUserWorkshopsNameQueryValidator : AbstractValidator<GetUserWorkshopsNameQuery>
{
    public GetUserWorkshopsNameQueryValidator()
    {
        RuleFor(x=>x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر اجباری است.");
    }
}