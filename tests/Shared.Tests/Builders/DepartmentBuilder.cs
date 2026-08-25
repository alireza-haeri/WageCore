namespace Shared.Tests.Builders;

public class DepartmentBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _workshopId = Guid.NewGuid();
    private string _name = "بخش نمونه";

    public DepartmentBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public DepartmentBuilder WithWorkshopId(Guid workshopId)
    {
        _workshopId = workshopId;
        return this;
    }

    public DepartmentBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public DomainResult<Department> CreateResult()
    {
        return Department.Create(_id, _workshopId, _name);
    }
}
