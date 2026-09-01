using Core.Contracts.Employees;

namespace Application.Validations;

public class EmployeeInformationValidator : AbstractValidator<EmployeeDto>
{
    public EmployeeInformationValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("شناسه بخش نمیتواند خالی باشد.");

        RuleFor(x => x.PersonalCode)
            .NotEmpty().WithMessage("کد پرسنلی اجباری است.")
            .Matches(RegexExtensions.ValidEmployeePersonalCodeRegex())
            .WithMessage("کد پرسنلی باید بین 1 تا 20 کاراکتر و فقط شامل حروف و اعداد انگلیسی باشد.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام و نام خانوادگی اجباری است.")
            .MinimumLength(3).WithMessage("نام و نام خانوادگی نمیتواند کمتر از 3 کاراکتر باشد.")
            .MaximumLength(100).WithMessage("نام و نام خانوادگی نمیتواند بیشتر از 100 کاراکتر باشد.")
            .WithName("نام و نام خانوادگی");

        RuleFor(x => x.NationalCode)
            .NotEmpty().WithMessage("کد ملی اجباری است.")
            .Matches(RegexExtensions.ValidNationalIdRegex())
            .WithMessage("کد ملی باید 10 رقم انگلیسی باشد.");

        RuleFor(x => x.FatherName)
            .NotEmpty().WithMessage("نام پدر اجباری است.")
            .MinimumLength(3).WithMessage("نام پدر نمیتواند کمتر از 3 کاراکتر باشد.")
            .MaximumLength(50).WithMessage("نام پدر نمیتواند بیشتر از 50 کاراکتر باشد.")
            .WithName("نام پدر");

        RuleFor(x => x.Gender)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("جنسیت اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("جنسیت معتبر نیست.");

        RuleFor(x => x.Region)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("منطقه کارمند اجباری است.")
            .Must(x => x.HasValue && Enum.IsDefined(x.Value))
            .WithMessage("منطقه کارمند معتبر نیست.");

        RuleFor(x => x.HireDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("تاریخ استخدام اجباری است.")
            .Must(x => x.HasValue && x.Value <= DateOnly.FromDateTime(DateTime.Now))
            .WithMessage("تاریخ استخدام نباید برای آینده باشد.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("شماره تلفن اجباری است.")
            .Matches(RegexExtensions.ValidIranianPhoneNumberRegex())
            .WithMessage("شماره تلفن باید با ۰۹ شروع شده و دقیقاً ۱۱ رقم انگلیسی باشد.");

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("عنوان شغلی نمیتواند بیشتر از 100 کاراکتر باشد.")
            .When(x => !string.IsNullOrWhiteSpace(x.JobTitle));
    }
}
