namespace Application.Features.Workshops;

public class GetWorkshopForEditQueryValidator : AbstractValidator<GetWorkshopForEditQuery>
{
    public GetWorkshopForEditQueryValidator()
    {
        RuleFor(x=>x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمی‌تواند خالی باشد.");
        
        RuleFor(x=>x.WorkshopId)
            .NotEmpty().WithMessage("شناسه کارگاه نمی‌تواند خالی باشد.");
    }
}