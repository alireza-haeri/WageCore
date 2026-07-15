using Shared.Kernel.Common;
using Shared.Kernel.Extensions;

namespace Core.Domain;

public class User
{
    public const string TableName = "Users";

    public Guid Id { get; private init; }
    public string PhoneNumber { get; private set; } = string.Empty;

    public static DomainResult<User> Create(Guid id, string? phoneNumber)
    {
        if (id == Guid.Empty)
            return DomainResult<User>.Failure("شناسه کاربر نمیتواند خالی باشد.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            return DomainResult<User>.Failure("شماره تلفن نمیتواند خالی باشد.");
        
        if(phoneNumber.Length != 11)
            return DomainResult<User>.Failure("شماره تلفن باید 11 رقم باشد.");
        
        if(!RegexExtensions.CheckPhoneNumberMustBeEnglishDigitsRegex().IsMatch(phoneNumber))
            return DomainResult<User>.Failure("شماره تلفن باید اعداد انگلیسی باشد.");

        return DomainResult<User>.Success(new User
        {
            Id = id,
            PhoneNumber = phoneNumber
        });
    }

    public static DomainResult<User> Create(string phoneNumber) => Create(Guid.NewGuid(), phoneNumber);
}