namespace Core.Domain;

public class Department
{
    public const string TableName = "Departments";
    public const int MaxDisplayNameLength = 20;

    public Guid Id { get; private init; }
    public Guid WorkshopId { get; private init; }
    public string Name { get; private set; } = null!;

    public static DomainResult<Department> Create(Guid departmentId, Guid workshopId, string name)
    {
        if (departmentId == Guid.Empty)
            return DomainResult<Department>.Failure("شناسه بخش نمیتواند خالی باشد.");

        if (workshopId == Guid.Empty)
            return DomainResult<Department>.Failure("شناسه کارگاه نمیتواند خالی باشد.");

        if (string.IsNullOrWhiteSpace(name))
            return DomainResult<Department>.Failure("نام بخش نمیتواند خالی باشد.");

        if (name.Length < 2 || name.Length > 100)
            return DomainResult<Department>.Failure("نام بخش باید بین 2 تا 100 حرف باشد.");

        return DomainResult<Department>.Success(new Department
        {
            Id = departmentId,
            WorkshopId = workshopId,
            Name = name
        });
    }

    public static DomainResult<Department> Create(Guid workshopId, string name) =>
        Create(Guid.NewGuid(), workshopId, name);

    public DomainResult Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainResult.Failure("نام بخش نمیتواند خالی باشد.");

        if (name.Length < 2 || name.Length > 100)
            return DomainResult.Failure("نام بخش باید بین 2 تا 100 حرف باشد.");

        Name = name;

        return DomainResult.Success();
    }
}
