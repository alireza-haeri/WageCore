namespace Application.Features.Departments;

public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر اجباری است.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("شناسه بخش اجباری است.");
    }
}
