namespace Application.Features.Workshops;

public class UpdateWorkshopCommandValidator : AbstractValidator<UpdateWorkshopCommand>
{
    public UpdateWorkshopCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("شناسه کاربر نمیتواند خالی باشد.");
        
        RuleFor(x => x.WorkshopId)
            .NotEmpty().WithMessage("شناسه کارگاه نمیتواند خالی باشد.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام کارگاه اجباری است.")
            .MinimumLength(2).WithMessage("نام کارگاه نمیتواند کمتر از 2 کاراکتر باشد.")
            .MaximumLength(200).WithMessage("نام کارگاه نمیتواند بیشتر از 200 کاراکتر باشد.")
            .WithName("نام کارگاه");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("آدرس کارگاه اجباری است.")
            .MinimumLength(10).WithMessage("آدرس کارگاه نمیتواند کمتر از 10 کاراکتر باشد.")
            .MaximumLength(1000).WithMessage("آدرس کارگاه نمیتواند بیشتر از 1000 کاراکتر باشد.")
            .WithName("آدرس کارگاه");

        RuleFor(x => x.Region)
            .IsInEnum().WithMessage("منطقه کارگاه معتبر نیست.");

        RuleFor(x => x.RegistrationDate)
            .NotEmpty().WithMessage("تاریخ ثبت کارگاه اجباری است.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("تاریخ ثبت کارگاه نباید برای آینده باشد.");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("شناسه ملی کارگاه اجباری است.")
            .Matches(RegexExtensions.ValidNationalIdRegex())
            .WithMessage("شناسه ملی کارگاه باید 10 رقم انگلیسی باشد.");

        RuleFor(x => x.PostalCode)
            .Matches(RegexExtensions.ValidPostalCodeRegex())
            .WithMessage("کد پستی باید 10 رقم انگلیسی باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.PostalCode));
    }
}