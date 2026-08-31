// WorkshopBuilder.cs
namespace Shared.Tests.Builders;

public class WorkshopBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private string _name = "کارگاه نمونه";
    private string _address = "تهران، خیابان نمونه، پلاک ۱۲۳";
    private DateOnly? _registrationDate = DateOnly.FromDateTime(DateTime.Now);
    private string _nationalId = "1234567890";
    private string? _postalCode = "1234567890";

    public WorkshopBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public WorkshopBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    public WorkshopBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public WorkshopBuilder WithAddress(string address)
    {
        _address = address;
        return this;
    }

    public WorkshopBuilder WithRegistrationDate(DateOnly? registrationDate)
    {
        _registrationDate = registrationDate;
        return this;
    }

    public WorkshopBuilder WithNationalId(string nationalId)
    {
        _nationalId = nationalId;
        return this;
    }

    public WorkshopBuilder WithPostalCode(string? postalCode)
    {
        _postalCode = postalCode;
        return this;
    }

    public DomainResult<Workshop> CreateResult()
    {
        return Workshop.Create(_id, _userId, _name, _address, _registrationDate, _nationalId, _postalCode);
    }
}