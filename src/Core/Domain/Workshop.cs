namespace Core.Domain;

public class Workshop
{
    public const string TableName = "Workshops";
    public const int MaxDisplayNameLength = 20;

    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public DateOnly RegistrationDate { get; private set; }
    public string NationalId { get; private set; } = null!;
    public string? PostalCode { get; private set; }
    public string SocialSecurityNumber { get; private set; } = null!;
    public string? EconomicCode { get; private set; }

    private readonly List<Department> _departments = [];
    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();

    public static DomainResult<Workshop> Create(Guid workshopId, Guid userId, string name, string address,
        DateOnly? registrationDate, string nationalId, string socialSecurityNumber,
        string? postalCode = null, string? economicCode = null)
    {
        if (workshopId == Guid.Empty)
            return DomainResult<Workshop>.Failure("شناسه کارگاه نمیتواند خالی باشد.");

        if (userId == Guid.Empty)
            return DomainResult<Workshop>.Failure("شناسه کاربر نمیتواند خالی باشد.");

        if (string.IsNullOrWhiteSpace(name))
            return DomainResult<Workshop>.Failure("نام کارگاه نمیتواند خالی باشد.");

        if (name.Length < 2 || name.Length > 200)
            return DomainResult<Workshop>.Failure("نام کارگاه باید بین 2 تا 200 حرف باشد.");

        if (string.IsNullOrWhiteSpace(address))
            return DomainResult<Workshop>.Failure("آدرس کارگاه نمیتواند خالی باشد.");

        if (address.Length < 10 || address.Length > 1000)
            return DomainResult<Workshop>.Failure("آدرس کارگاه باید بین 10 تا 1000 حرف باشد.");

        if (registrationDate is null)
            return DomainResult<Workshop>.Failure("تاریخ ثبت کارگاه نمیتواند خالی باشد.");

        if (registrationDate > DateOnly.FromDateTime(DateTime.Now))
            return DomainResult<Workshop>.Failure("تاریخ ثبت کارگاه نباید برای آینده باشد.");

        if (string.IsNullOrWhiteSpace(nationalId))
            return DomainResult<Workshop>.Failure("شناسه ملی کارگاه نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidNationalIdRegex().IsMatch(nationalId))
            return DomainResult<Workshop>.Failure("شناسه ملی کارگاه باید 10 رقم انگلیسی باشد.");

        if (!string.IsNullOrWhiteSpace(postalCode))
        {
            if (!RegexExtensions.ValidPostalCodeRegex().IsMatch(postalCode))
                return DomainResult<Workshop>.Failure("کد پستی باید 10 رقم انگلیسی باشد.");
        }
        else
            postalCode = null;

        if (string.IsNullOrWhiteSpace(socialSecurityNumber))
            return DomainResult<Workshop>.Failure("شماره بیمه تامین اجتماعی کارگاه نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidSocialSecurityNumberRegex().IsMatch(socialSecurityNumber))
            return DomainResult<Workshop>.Failure("شماره بیمه تامین اجتماعی کارگاه باید 1 تا 20 رقم انگلیسی باشد.");

        if (!string.IsNullOrWhiteSpace(economicCode))
        {
            if (!RegexExtensions.ValidEconomicCodeRegex().IsMatch(economicCode))
                return DomainResult<Workshop>.Failure("شماره اقتصادی کارگاه باید 1 تا 20 رقم انگلیسی باشد.");
        }
        else
            economicCode = null;

        var workshop = new Workshop
        {
            Id = workshopId,
            UserId = userId,
            Name = name,
            Address = address,
            RegistrationDate = registrationDate.Value,
            NationalId = nationalId,
            PostalCode = postalCode,
            SocialSecurityNumber = socialSecurityNumber,
            EconomicCode = economicCode
        };
        return DomainResult<Workshop>.Success(workshop);
    }

    public static DomainResult<Workshop> Create(Guid userId, string name, string address,
        DateOnly? registrationDate, string nationalId, string socialSecurityNumber,
        string? postalCode = null, string? economicCode = null) =>
        Create(Guid.NewGuid(), userId, name, address, registrationDate, nationalId, socialSecurityNumber,
            postalCode, economicCode);

    public DomainResult Update(string name, string address, DateOnly? registrationDate,
        string nationalId, string socialSecurityNumber, string? postalCode = null,
        string? economicCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainResult.Failure("نام کارگاه نمیتواند خالی باشد.");

        if (name.Length < 2 || name.Length > 200)
            return DomainResult.Failure("نام کارگاه باید بین 2 تا 200 حرف باشد.");

        if (string.IsNullOrWhiteSpace(address))
            return DomainResult.Failure("آدرس کارگاه نمیتواند خالی باشد.");

        if (address.Length < 10 || address.Length > 1000)
            return DomainResult.Failure("آدرس کارگاه باید بین 10 تا 1000 حرف باشد.");


        if (registrationDate is null)
            return DomainResult.Failure("تاریخ ثبت کارگاه نمیتواند خالی باشد.");

        if (registrationDate > DateOnly.FromDateTime(DateTime.Now))
            return DomainResult.Failure("تاریخ ثبت کارگاه نباید برای آینده باشد.");

        if (string.IsNullOrWhiteSpace(nationalId))
            return DomainResult.Failure("شناسه ملی کارگاه نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidNationalIdRegex().IsMatch(nationalId))
            return DomainResult.Failure("شناسه ملی کارگاه باید 10 رقم انگلیسی باشد.");

        if (!string.IsNullOrWhiteSpace(postalCode))
        {
            if (!RegexExtensions.ValidPostalCodeRegex().IsMatch(postalCode))
                return DomainResult.Failure("کد پستی باید 10 رقم انگلیسی باشد.");
        }
        else
            postalCode = null;

        if (string.IsNullOrWhiteSpace(socialSecurityNumber))
            return DomainResult.Failure("شماره بیمه تامین اجتماعی کارگاه نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidSocialSecurityNumberRegex().IsMatch(socialSecurityNumber))
            return DomainResult.Failure("شماره بیمه تامین اجتماعی کارگاه باید 1 تا 20 رقم انگلیسی باشد.");

        if (!string.IsNullOrWhiteSpace(economicCode))
        {
            if (!RegexExtensions.ValidEconomicCodeRegex().IsMatch(economicCode))
                return DomainResult.Failure("شماره اقتصادی کارگاه باید 1 تا 20 رقم انگلیسی باشد.");
        }
        else
            economicCode = null;

        Name = name;
        Address = address;
        RegistrationDate = registrationDate.Value;
        NationalId = nationalId;
        PostalCode = postalCode;
        SocialSecurityNumber = socialSecurityNumber;
        EconomicCode = economicCode;

        return DomainResult.Success();
    }

    public DomainResult<Department> CreateDepartment(Guid departmentId, string name)
    {
        var departmentResult = Department.Create(departmentId, Id, name);
        if (!departmentResult.IsSuccess)
            return DomainResult<Department>.Failure(departmentResult.ErrorMessage!);

        _departments.Add(departmentResult.Response);
        return departmentResult;
    }

    public DomainResult<Department> CreateDepartment(string name) =>
        CreateDepartment(Guid.NewGuid(), name);

    public DomainResult UpdateDepartment(Guid departmentId, string name)
    {
        var department = _departments.FirstOrDefault(x => x.Id == departmentId);
        if (department is null)
            return DomainResult.Failure("بخش مورد نظر یافت نشد.");

        return department.Update(name);
    }

    public DomainResult DeleteDepartment(Guid departmentId)
    {
        var department = _departments.FirstOrDefault(x => x.Id == departmentId);
        if (department is null)
            return DomainResult.Failure("بخش مورد نظر یافت نشد.");

        _departments.Remove(department);
        return DomainResult.Success();
    }
}