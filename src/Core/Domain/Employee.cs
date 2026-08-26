namespace Core.Domain;

public class Employee
{
    public const string TableName = "Employees";

    public Guid Id { get; private init; }
    public Guid WorkshopId { get; private init; }
    public Guid DepartmentId { get; private set; }
    public string PersonalCode { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string NationalCode { get; private set; } = null!;
    public string BirthCertificateNumber { get; private set; } = null!;
    public string FatherName { get; private set; } = null!;
    public EmployeeGender Gender { get; private set; }
    public EmployeeMaritalStatus MaritalStatus { get; private set; }
    public int ChildrenCount { get; private set; }
    public DateOnly HireDate { get; private set; }
    public DateOnly? TerminationDate { get; private set; }
    public string PhoneNumber { get; private set; } = null!;
    public string? JobTitle { get; private set; }
    public bool IsTaxSubject { get; private set; }
    public Insurance Insurance { get; private set; } = null!;
    public bool IsTerminated => TerminationDate.HasValue;

    private readonly List<BankAccount> _bankAccounts = [];
    public IReadOnlyCollection<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    public static DomainResult<Employee> Create(
        Guid employeeId,
        Guid workshopId,
        DateOnly? workshopRegistrationDate,
        EmployeeDto? employee,
        EmployeeInsuranceDto? insurance,
        bool isPersonalCodeUniqueForUser = true,
        bool isNationalCodeUniqueForUser = true)
    {
        var validationResult = Validate(
            employeeId,
            workshopId,
            workshopRegistrationDate,
            employee,
            isPersonalCodeUniqueForUser,
            isNationalCodeUniqueForUser);

        if (!validationResult.IsSuccess)
            return DomainResult<Employee>.Failure(validationResult.ErrorMessage!);

        var insuranceResult = Insurance.Create(insurance);
        if (!insuranceResult.IsSuccess)
            return DomainResult<Employee>.Failure(insuranceResult.ErrorMessage!);

        return DomainResult<Employee>.Success(new Employee
        {
            Id = employeeId,
            WorkshopId = workshopId,
            DepartmentId = employee!.DepartmentId,
            PersonalCode = employee.PersonalCode,
            FullName = employee.FullName,
            NationalCode = employee.NationalCode,
            BirthCertificateNumber = employee.BirthCertificateNumber,
            FatherName = employee.FatherName,
            Gender = employee.Gender!.Value,
            MaritalStatus = employee.MaritalStatus!.Value,
            ChildrenCount = employee.ChildrenCount!.Value,
            HireDate = employee.HireDate!.Value,
            PhoneNumber = employee.PhoneNumber,
            JobTitle = NormalizeJobTitle(employee.JobTitle),
            IsTaxSubject = employee.IsTaxSubject,
            Insurance = insuranceResult.Response
        });
    }

    public static DomainResult<Employee> Create(
        Guid workshopId,
        DateOnly? workshopRegistrationDate,
        EmployeeDto? employee,
        EmployeeInsuranceDto? insurance,
        bool isPersonalCodeUniqueForUser = true,
        bool isNationalCodeUniqueForUser = true) =>
        Create(
            Guid.NewGuid(),
            workshopId,
            workshopRegistrationDate,
            employee,
            insurance,
            isPersonalCodeUniqueForUser,
            isNationalCodeUniqueForUser);

    public DomainResult Update(
        EmployeeDto? employee,
        DateOnly? workshopRegistrationDate,
        bool isPersonalCodeUniqueForUser = true,
        bool isNationalCodeUniqueForUser = true)
    {
        var canModifyResult = EnsureCanModify();
        if (!canModifyResult.IsSuccess)
            return canModifyResult;

        var validationResult = Validate(
            Id,
            WorkshopId,
            workshopRegistrationDate,
            employee,
            (employee is not null &&
             string.Equals(PersonalCode, employee.PersonalCode, StringComparison.OrdinalIgnoreCase)) ||
            isPersonalCodeUniqueForUser,
            (employee is not null &&
             string.Equals(NationalCode, employee.NationalCode, StringComparison.Ordinal)) ||
            isNationalCodeUniqueForUser);

        if (!validationResult.IsSuccess)
            return validationResult;

        DepartmentId = employee!.DepartmentId;
        PersonalCode = employee.PersonalCode;
        FullName = employee.FullName;
        NationalCode = employee.NationalCode;
        BirthCertificateNumber = employee.BirthCertificateNumber;
        FatherName = employee.FatherName;
        Gender = employee.Gender!.Value;
        MaritalStatus = employee.MaritalStatus!.Value;
        ChildrenCount = employee.ChildrenCount!.Value;
        HireDate = employee.HireDate!.Value;
        PhoneNumber = employee.PhoneNumber;
        JobTitle = NormalizeJobTitle(employee.JobTitle);
        IsTaxSubject = employee.IsTaxSubject;

        return DomainResult.Success();
    }

    public DomainResult Terminate(DateOnly? terminationDate)
    {
        if (terminationDate is null)
            return DomainResult.Failure("تاریخ ترک کار نمیتواند خالی باشد.");

        if (TerminationDate is not null)
            return DomainResult.Failure("کارمند قبلاً ترک کار شده است.");

        if (terminationDate > DateOnly.FromDateTime(DateTime.Now))
            return DomainResult.Failure("تاریخ ترک کار نباید برای آینده باشد.");

        if (terminationDate < HireDate)
            return DomainResult.Failure("تاریخ ترک کار نباید قبل از تاریخ استخدام باشد.");

        TerminationDate = terminationDate.Value;
        return DomainResult.Success();
    }

    public DomainResult Rehire(EmployeeRehireDto? rehire)
    {
        if (TerminationDate is null)
            return DomainResult.Failure("تنها کارمند ترک کار شده را میتوان دوباره استخدام کرد.");

        if (rehire is null)
            return DomainResult.Failure("اطلاعات استخدام مجدد نمیتواند خالی باشد.");

        if (rehire.DepartmentId == Guid.Empty)
            return DomainResult.Failure("شناسه بخش نمیتواند خالی باشد.");

        if (rehire.WorkshopRegistrationDate is null)
            return DomainResult.Failure("تاریخ ثبت کارگاه نمیتواند خالی باشد.");

        if (rehire.HireDate is null)
            return DomainResult.Failure("تاریخ استخدام نمیتواند خالی باشد.");

        if (rehire.HireDate > DateOnly.FromDateTime(DateTime.Now))
            return DomainResult.Failure("تاریخ استخدام نباید برای آینده باشد.");

        if (rehire.HireDate < rehire.WorkshopRegistrationDate)
            return DomainResult.Failure("تاریخ استخدام نباید قبل از تاریخ ثبت کارگاه باشد.");

        if (rehire.HireDate <= TerminationDate)
            return DomainResult.Failure("تاریخ استخدام مجدد باید بعد از تاریخ ترک کار باشد.");

        DepartmentId = rehire.DepartmentId;
        HireDate = rehire.HireDate.Value;
        TerminationDate = null;

        return DomainResult.Success();
    }

    public DomainResult UpdateInsurance(EmployeeInsuranceDto? insurance)
    {
        var canModifyResult = EnsureCanModify();
        if (!canModifyResult.IsSuccess)
            return canModifyResult;

        return Insurance.Update(insurance);
    }

    public DomainResult ReplaceBankAccounts(List<EmployeeBankAccountDto>? bankAccounts)
    {
        var canModifyResult = EnsureCanModify();
        if (!canModifyResult.IsSuccess)
            return canModifyResult;

        if (bankAccounts is null)
            return DomainResult.Failure("اطلاعات حساب‌های بانکی نمیتواند خالی باشد.");

        if (bankAccounts.Count == 0)
            return DomainResult.Failure("کارمند باید حداقل یک حساب بانکی داشته باشد.");

        var normalizedBankAccounts = new List<BankAccount>();

        foreach (var bankAccount in bankAccounts)
        {
            var bankAccountId = bankAccount.Id ?? Guid.NewGuid();
            if (normalizedBankAccounts.Any(x => x.Id == bankAccountId))
                return DomainResult.Failure("شناسه حساب بانکی در لیست حساب‌های بانکی تکراری است.");

            var bankAccountResult = BankAccount.Create(bankAccountId, bankAccount);
            if (!bankAccountResult.IsSuccess)
                return DomainResult.Failure(bankAccountResult.ErrorMessage!);

            if (normalizedBankAccounts.Any(x => x.Iban == bankAccountResult.Response.Iban))
                return DomainResult.Failure("شماره شبا در لیست حساب‌های بانکی تکراری است.");

            normalizedBankAccounts.Add(bankAccountResult.Response);
        }

        _bankAccounts.Clear();
        _bankAccounts.AddRange(normalizedBankAccounts);

        return DomainResult.Success();
    }

    private static DomainResult Validate(
        Guid employeeId,
        Guid workshopId,
        DateOnly? workshopRegistrationDate,
        EmployeeDto? employee,
        bool isPersonalCodeUniqueForUser,
        bool isNationalCodeUniqueForUser)
    {
        if (employeeId == Guid.Empty)
            return DomainResult.Failure("شناسه کارمند نمیتواند خالی باشد.");

        if (workshopId == Guid.Empty)
            return DomainResult.Failure("شناسه کارگاه نمیتواند خالی باشد.");

        if (employee is null)
            return DomainResult.Failure("اطلاعات کارمند نمیتواند خالی باشد.");

        if (employee.DepartmentId == Guid.Empty)
            return DomainResult.Failure("شناسه بخش نمیتواند خالی باشد.");

        if (string.IsNullOrWhiteSpace(employee.PersonalCode))
            return DomainResult.Failure("کد پرسنلی نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidEmployeePersonalCodeRegex().IsMatch(employee.PersonalCode))
            return DomainResult.Failure("کد پرسنلی باید بین 1 تا 20 کاراکتر و فقط شامل حروف و اعداد انگلیسی باشد.");

        if (!isPersonalCodeUniqueForUser)
            return DomainResult.Failure("کد پرسنلی در بین کارکنان این کاربر تکراری است.");

        if (string.IsNullOrWhiteSpace(employee.FullName))
            return DomainResult.Failure("نام و نام خانوادگی نمیتواند خالی باشد.");

        if (employee.FullName.Length < 3 || employee.FullName.Length > 100)
            return DomainResult.Failure("نام و نام خانوادگی باید بین 3 تا 100 حرف باشد.");

        if (string.IsNullOrWhiteSpace(employee.NationalCode))
            return DomainResult.Failure("کد ملی نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidNationalIdRegex().IsMatch(employee.NationalCode))
            return DomainResult.Failure("کد ملی باید 10 رقم انگلیسی باشد.");

        if (!isNationalCodeUniqueForUser)
            return DomainResult.Failure("کد ملی در بین کارکنان این کاربر تکراری است.");

        if (string.IsNullOrWhiteSpace(employee.BirthCertificateNumber))
            return DomainResult.Failure("شماره شناسنامه نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidBirthCertificateNumberRegex().IsMatch(employee.BirthCertificateNumber))
            return DomainResult.Failure("شماره شناسنامه باید بین 1 تا 20 رقم انگلیسی باشد.");

        if (string.IsNullOrWhiteSpace(employee.FatherName))
            return DomainResult.Failure("نام پدر نمیتواند خالی باشد.");

        if (employee.FatherName.Length < 3 || employee.FatherName.Length > 50)
            return DomainResult.Failure("نام پدر باید بین 3 تا 50 حرف باشد.");

        if (employee.Gender is null)
            return DomainResult.Failure("جنسیت نمیتواند خالی باشد.");

        if (employee.MaritalStatus is null)
            return DomainResult.Failure("وضعیت تاهل نمیتواند خالی باشد.");

        if (employee.ChildrenCount is null)
            return DomainResult.Failure("تعداد فرزندان نمیتواند خالی باشد.");

        if (employee.ChildrenCount < 0 || employee.ChildrenCount > 20)
            return DomainResult.Failure("تعداد فرزندان باید بین 0 تا 20 باشد.");

        if (employee.MaritalStatus == EmployeeMaritalStatus.Single && employee.ChildrenCount > 0)
            return DomainResult.Failure("برای کارمند مجرد، تعداد فرزندان باید صفر باشد.");

        if (workshopRegistrationDate is null)
            return DomainResult.Failure("تاریخ ثبت کارگاه نمیتواند خالی باشد.");

        if (employee.HireDate is null)
            return DomainResult.Failure("تاریخ استخدام نمیتواند خالی باشد.");

        if (employee.HireDate > DateOnly.FromDateTime(DateTime.Now))
            return DomainResult.Failure("تاریخ استخدام نباید برای آینده باشد.");

        if (employee.HireDate < workshopRegistrationDate)
            return DomainResult.Failure("تاریخ استخدام نباید قبل از تاریخ ثبت کارگاه باشد.");

        if (string.IsNullOrWhiteSpace(employee.PhoneNumber))
            return DomainResult.Failure("شماره تلفن نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidIranianPhoneNumberRegex().IsMatch(employee.PhoneNumber))
            return DomainResult.Failure("شماره تلفن باید با ۰۹ شروع شده و دقیقاً ۱۱ رقم انگلیسی باشد.");

        if (!string.IsNullOrWhiteSpace(employee.JobTitle) && employee.JobTitle.Length > 100)
            return DomainResult.Failure("عنوان شغلی نمیتواند بیشتر از 100 حرف باشد.");

        return DomainResult.Success();
    }

    private DomainResult EnsureCanModify() =>
        TerminationDate is null
            ? DomainResult.Success()
            : DomainResult.Failure("کارمند ترک کار شده است و فقط امکان حذف یا استخدام مجدد برای او وجود دارد.");

    private static string? NormalizeJobTitle(string? jobTitle) =>
        string.IsNullOrWhiteSpace(jobTitle) ? null : jobTitle;
}
