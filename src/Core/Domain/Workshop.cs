namespace Core.Domain;

public class Workshop
{
    public const string TableName = "Workshops";
    public const int MaxDisplayNameLength = 20;

    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public WorkshopRegion Region { get; private set; }
    public DateOnly RegistrationDate { get; private set; }
    public string NationalId { get; private set; } = null!;
    public string? PostalCode { get; private set; }

    public static DomainResult<Workshop> Create(Guid workshopId, Guid userId, string name, string address,
        WorkshopRegion? region,
        DateOnly? registrationDate, string nationalId, string? postalCode = null)
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

        if (region is null)
            return DomainResult<Workshop>.Failure("منطقه کارگاه نمیتواند خالی باشد.");

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

        var workshop = new Workshop
        {
            Id = workshopId,
            UserId = userId,
            Name = name,
            Address = address,
            Region = region.Value,
            RegistrationDate = registrationDate.Value,
            NationalId = nationalId,
            PostalCode = postalCode
        };
        return DomainResult<Workshop>.Success(workshop);
    }

    public static DomainResult<Workshop> Create(Guid userId, string name, string address, WorkshopRegion? region,
        DateOnly? registrationDate, string nationalId, string? postalCode = null) =>
        Create(Guid.NewGuid(), userId, name, address, region, registrationDate, nationalId, postalCode);

    public DomainResult Update(string name, string address, WorkshopRegion? region, DateOnly? registrationDate,
        string nationalId, string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainResult.Failure("نام کارگاه نمیتواند خالی باشد.");

        if (name.Length < 2 || name.Length > 200)
            return DomainResult.Failure("نام کارگاه باید بین 2 تا 200 حرف باشد.");

        if (string.IsNullOrWhiteSpace(address))
            return DomainResult.Failure("آدرس کارگاه نمیتواند خالی باشد.");

        if (address.Length < 10 || address.Length > 1000)
            return DomainResult.Failure("آدرس کارگاه باید بین 10 تا 1000 حرف باشد.");

        if (region is null)
            return DomainResult.Failure("منطقه کارگاه نمیتواند خالی باشد.");

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
        
        Name = name;
        Address = address;
        Region = region.Value;
        RegistrationDate = registrationDate.Value;
        NationalId = nationalId;
        PostalCode = postalCode;

        return DomainResult.Success();
    }
}

public enum WorkshopRegion
{
    Normal = 0,
    LessDeveloped = 1,
}