namespace Application.Features.Departments;

public class GetUserDepartmentsNameQueryValidator : AbstractValidator<GetUserDepartmentsNameQuery>
{
    public GetUserDepartmentsNameQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر اجباری است.");
    }
}
