namespace Shared.Tests.Builders;

public class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string? _phoneNumber = "09123456789";

    public UserBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UserBuilder WithPhoneNumber(string? phoneNumber)
    {
        _phoneNumber = phoneNumber;
        return this;
    }

    public DomainResult<User> CreateResult()
    {
        return User.Create(_id, _phoneNumber);
    }
}