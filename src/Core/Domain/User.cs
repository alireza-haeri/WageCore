using System.ComponentModel.DataAnnotations;
using Shared.Kernel.Common;
using Shared.Kernel.Extensions;

namespace Core.Domain;

public class User
{
    public Guid Id { get; private init; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string FullName { get; private set; } = string.Empty;

    public static DomainResult<User> Create(Guid id, string? phoneNumber, string? email, string? fullName)
    {
        if (id == Guid.Empty)
            return DomainResult<User>.Failure("شناسه کاربر نمیتواند خالی باشد.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            phoneNumber = null;
        
        if (string.IsNullOrWhiteSpace(email))
            email = null;
        
        if (phoneNumber is null && email is null)
            return DomainResult<User>.Failure("حداقل یکی از فیلدهای شماره تلفن یا ایمیل باید وارد شود.");

        if (phoneNumber is not null)
            if (!RegexExtensions.ValidIranianPhoneNumberRegex().IsMatch(phoneNumber))
                return DomainResult<User>.Failure("شماره تلفن باید با ۰۹ شروع شده و دقیقاً ۱۱ رقم انگلیسی باشد.");

        if (email is not null)
            if (!RegexExtensions.ValidEmailRegex().IsMatch(email))
                return DomainResult<User>.Failure("فرمت ایمیل را درست وارد کنید.");

        if (string.IsNullOrWhiteSpace(fullName))
            return DomainResult<User>.Failure("نام و نام خانوادگی نمیتواند خالی باشد.");

        if (fullName.Length < 3)
            return DomainResult<User>.Failure("نام و نام خانوادگی نمیتواند کمتر از 3 حرف باشد.");

        if (fullName.Length > 100)
            return DomainResult<User>.Failure("نام و نام خانوادگی نمیتواند بیشتر از 100 حرف باشد.");

        return DomainResult<User>.Success(new User
        {
            Id = id,
            PhoneNumber = phoneNumber,
            Email = email,
            FullName = fullName
        });
    }

    public static DomainResult<User> Create(string? phoneNumber, string? email, string? fullName) =>
        Create(Guid.NewGuid(), phoneNumber, email, fullName);
}