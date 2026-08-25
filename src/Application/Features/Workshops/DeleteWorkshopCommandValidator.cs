namespace Application.Features.Workshops;

public class DeleteWorkshopCommandValidator : AbstractValidator<DeleteWorkshopCommand>
{
    public DeleteWorkshopCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر اجباری است.");
        
        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("شناسه کارگاه اجباری است.");
    }    
}