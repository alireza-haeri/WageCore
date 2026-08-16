namespace Shared.Tests.Builders;

public class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string? _phoneNumber = "09123456789";
    private string? _email = "ali@gmail.com";
    private string? _fullName = "ali rezay";

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

    public UserBuilder WithEmail(string? email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithFullName(string? fullName)
    {
        _fullName = fullName;
        return this;
    }

    public DomainResult<User> CreateResult()
    {
        return User.Create(_id, _phoneNumber,_email, _fullName);
    }
}