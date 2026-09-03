using Core.Abstractions.Services;

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
    public string FatherName { get; private set; } = null!;
    public EmployeeGender Gender { get; private set; }
    public DateOnly HireDate { get; private set; }
    public DateOnly? TerminationDate { get; private set; }
    public string PhoneNumber { get; private set; } = null!;
    public string? JobTitle { get; private set; }
    public Region Region { get; private set; }

    /// <summary>تعداد مرخصی استفاده‌شده در سال جاری (سال استخدام) — فقط برای استخدام در سال جاری و قبل از ماه جاری.</summary>
    public int? LeaveUsedInCurrentYear { get; private set; }

    /// <summary>تعداد روز خالص کارکرد روزانه قبل از ماه جاری — فقط برای استخدام در سال جاری و قبل از ماه جاری.</summary>
    public int? NetWorkedDaysBeforeCurrentMonth { get; private set; }

    /// <summary>تعداد مرخصی انتقال‌یافته از سال قبل — فقط برای استخدام قبل از سال جاری.</summary>
    public int? CarriedOverLeaveFromPreviousYear { get; private set; }

    public bool IsTerminated => TerminationDate.HasValue;

    private readonly List<BankAccount> _bankAccounts = [];
    public IReadOnlyCollection<BankAccount> BankAccounts => _bankAccounts.AsReadOnly();

    public static DomainResult<Employee> Create(
        Guid employeeId,
        Guid workshopId,
        DateOnly? workshopRegistrationDate,
        EmployeeDto? employee,
        bool isPersonalCodeUniqueForUser = true,
        bool isNationalCodeUniqueForUser = true,
        IPersianCalendarService? persianCalendarService = null)
    {
        var validationResult = Validate(
            employeeId,
            workshopId,
            workshopRegistrationDate,
            employee,
            isPersonalCodeUniqueForUser,
            isNationalCodeUniqueForUser,
            persianCalendarService);

        if (!validationResult.IsSuccess)
            return DomainResult<Employee>.Failure(validationResult.ErrorMessage!);

        return DomainResult<Employee>.Success(new Employee
        {
            Id = employeeId,
            WorkshopId = workshopId,
            DepartmentId = employee!.DepartmentId,
            PersonalCode = employee.PersonalCode,
            FullName = employee.FullName,
            NationalCode = employee.NationalCode,
            FatherName = employee.FatherName,
            Gender = employee.Gender!.Value,
            HireDate = employee.HireDate!.Value,
            PhoneNumber = employee.PhoneNumber,
            JobTitle = NormalizeJobTitle(employee.JobTitle),
            Region = employee.Region!.Value,
            LeaveUsedInCurrentYear = employee.LeaveUsedInCurrentYear,
            NetWorkedDaysBeforeCurrentMonth = employee.NetWorkedDaysBeforeCurrentMonth,
            CarriedOverLeaveFromPreviousYear = employee.CarriedOverLeaveFromPreviousYear
        });
    }

    public static DomainResult<Employee> Create(
        Guid workshopId,
        DateOnly? workshopRegistrationDate,
        EmployeeDto? employee,
        bool isPersonalCodeUniqueForUser = true,
        bool isNationalCodeUniqueForUser = true,
        IPersianCalendarService? persianCalendarService = null) =>
        Create(
            Guid.NewGuid(),
            workshopId,
            workshopRegistrationDate,
            employee,
            isPersonalCodeUniqueForUser,
            isNationalCodeUniqueForUser,
            persianCalendarService);

    public DomainResult Update(
        EmployeeDto? employee,
        DateOnly? workshopRegistrationDate,
        bool isPersonalCodeUniqueForUser = true,
        bool isNationalCodeUniqueForUser = true,
        IPersianCalendarService? persianCalendarService = null)
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
            isNationalCodeUniqueForUser,
            persianCalendarService);

        if (!validationResult.IsSuccess)
            return validationResult;

        DepartmentId = employee!.DepartmentId;
        PersonalCode = employee.PersonalCode;
        FullName = employee.FullName;
        NationalCode = employee.NationalCode;
        FatherName = employee.FatherName;
        Gender = employee.Gender!.Value;
        HireDate = employee.HireDate!.Value;
        PhoneNumber = employee.PhoneNumber;
        JobTitle = NormalizeJobTitle(employee.JobTitle);
        Region = employee.Region!.Value;
        LeaveUsedInCurrentYear = employee.LeaveUsedInCurrentYear;
        NetWorkedDaysBeforeCurrentMonth = employee.NetWorkedDaysBeforeCurrentMonth;
        CarriedOverLeaveFromPreviousYear = employee.CarriedOverLeaveFromPreviousYear;

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

    public DomainResult EnsureEmployedDuring(DateOnly periodStart, DateOnly periodEnd)
    {
        if (periodEnd < HireDate)
            return DomainResult.Failure("کارمند در این بازه استخدام نشده بود.");

        if (TerminationDate is not null && TerminationDate < periodStart)
            return DomainResult.Failure("کارمند قبل از این بازه ترک کار کرده است.");

        return DomainResult.Success();
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
        bool isNationalCodeUniqueForUser,
        IPersianCalendarService? persianCalendarService)
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

        if (string.IsNullOrWhiteSpace(employee.FatherName))
            return DomainResult.Failure("نام پدر نمیتواند خالی باشد.");

        if (employee.FatherName.Length < 3 || employee.FatherName.Length > 50)
            return DomainResult.Failure("نام پدر باید بین 3 تا 50 حرف باشد.");

        if (employee.Gender is null)
            return DomainResult.Failure("جنسیت نمیتواند خالی باشد.");

        if (employee.Region is null)
            return DomainResult.Failure("منطقه کارمند نمیتواند خالی باشد.");

        if (!Enum.IsDefined(typeof(Region), employee.Region.Value))
            return DomainResult.Failure("منطقه کارمند معتبر نیست.");

        if (workshopRegistrationDate is null)
            return DomainResult.Failure("تاریخ ثبت کارگاه نمیتواند خالی باشد.");

        if (employee.HireDate is null)
            return DomainResult.Failure("تاریخ استخدام نمیتواند خالی باشد.");

        if (employee.HireDate > DateOnly.FromDateTime(DateTime.Now))
            return DomainResult.Failure("تاریخ استخدام نباید برای آینده باشد.");

        if (employee.HireDate < workshopRegistrationDate)
            return DomainResult.Failure("تاریخ استخدام نباید قبل از تاریخ ثبت کارگاه باشد.");

        if (persianCalendarService is not null)
        {
            var onboardingHistoryResult = ValidateOnboardingHistory(
                persianCalendarService,
                employee.HireDate.Value,
                employee.LeaveUsedInCurrentYear,
                employee.NetWorkedDaysBeforeCurrentMonth,
                employee.CarriedOverLeaveFromPreviousYear);
            if (!onboardingHistoryResult.IsSuccess)
                return onboardingHistoryResult;
        }

        if (string.IsNullOrWhiteSpace(employee.PhoneNumber))
            return DomainResult.Failure("شماره تلفن نمیتواند خالی باشد.");

        if (!RegexExtensions.ValidIranianPhoneNumberRegex().IsMatch(employee.PhoneNumber))
            return DomainResult.Failure("شماره تلفن باید با ۰۹ شروع شده و دقیقاً ۱۱ رقم انگلیسی باشد.");

        if (!string.IsNullOrWhiteSpace(employee.JobTitle) && employee.JobTitle.Length > 100)
            return DomainResult.Failure("عنوان شغلی نمیتواند بیشتر از 100 حرف باشد.");

        return DomainResult.Success();
    }

    /// <summary>
    /// Checks the onboarding-history fields against the hire date (Persian calendar):
    /// <list type="bullet">
    /// <item>Hired in the current month → all three fields must be null.</item>
    /// <item>Hired earlier this year → LeaveUsedInCurrentYear + NetWorkedDaysBeforeCurrentMonth
    /// must be present and non-negative; CarriedOverLeaveFromPreviousYear must be null.</item>
    /// <item>Hired before the current year → all three fields must be present and non-negative.</item>
    /// </list>
    /// </summary>
    public static DomainResult ValidateOnboardingHistory(
        IPersianCalendarService persianCalendarService,
        DateOnly hireDate,
        int? leaveUsedInCurrentYear,
        int? netWorkedDaysBeforeCurrentMonth,
        int? carriedOverLeaveFromPreviousYear)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var hireYear = persianCalendarService.GetPersianYear(hireDate);
        var hireMonth = persianCalendarService.GetPersianMonth(hireDate);
        var currentYear = persianCalendarService.GetPersianYear(today);
        var currentMonth = persianCalendarService.GetPersianMonth(today);

        bool requiresCurrentYearFields = true; // LeaveUsedInCurrentYear + NetWorkedDaysBeforeCurrentMonth
        bool requiresCarriedOver = false;      // CarriedOverLeaveFromPreviousYear

        if (hireYear == currentYear && hireMonth == currentMonth)
        {
            requiresCurrentYearFields = false;
            requiresCarriedOver = false;
        }
        else if (hireYear == currentYear)
        {
            requiresCurrentYearFields = true;
            requiresCarriedOver = false;
        }
        else // hireYear < currentYear (future dates are rejected earlier)
        {
            requiresCurrentYearFields = true;
            requiresCarriedOver = true;
        }

        if (!requiresCurrentYearFields)
        {
            if (leaveUsedInCurrentYear is not null)
                return DomainResult.Failure("کارمند همین ماه استخدام شده است؛ تعداد مرخصی استفاده‌شده در سال جاری ثبت نمی‌شود.");

            if (netWorkedDaysBeforeCurrentMonth is not null)
                return DomainResult.Failure("کارمند همین ماه استخدام شده است؛ روز خالص کارکرد قبل از ماه جاری ثبت نمی‌شود.");
        }
        else
        {
            if (leaveUsedInCurrentYear is null)
                return DomainResult.Failure("تعداد مرخصی استفاده‌شده در سال جاری اجباری است.");

            if (leaveUsedInCurrentYear.Value < 0)
                return DomainResult.Failure("تعداد مرخصی استفاده‌شده در سال جاری نمی‌تواند منفی باشد.");

            if (netWorkedDaysBeforeCurrentMonth is null)
                return DomainResult.Failure("تعداد روز خالص کارکرد قبل از ماه جاری اجباری است.");

            if (netWorkedDaysBeforeCurrentMonth.Value < 0)
                return DomainResult.Failure("تعداد روز خالص کارکرد قبل از ماه جاری نمی‌تواند منفی باشد.");
        }

        if (!requiresCarriedOver)
        {
            if (carriedOverLeaveFromPreviousYear is not null)
                return DomainResult.Failure("کارمند قبل از سال جاری استخدام نشده است؛ مرخصی انتقال‌یافته از سال قبل ثبت نمی‌شود.");
        }
        else
        {
            if (carriedOverLeaveFromPreviousYear is null)
                return DomainResult.Failure("تعداد مرخصی انتقال‌یافته از سال قبل اجباری است.");

            if (carriedOverLeaveFromPreviousYear.Value < 0)
                return DomainResult.Failure("تعداد مرخصی انتقال‌یافته از سال قبل نمی‌تواند منفی باشد.");
        }

        return DomainResult.Success();
    }

    private DomainResult EnsureCanModify() =>
        TerminationDate is null
            ? DomainResult.Success()
            : DomainResult.Failure("کارمند ترک کار شده است و فقط امکان حذف یا استخدام مجدد برای او وجود دارد.");

    private static string? NormalizeJobTitle(string? jobTitle) =>
        string.IsNullOrWhiteSpace(jobTitle) ? null : jobTitle;
}
