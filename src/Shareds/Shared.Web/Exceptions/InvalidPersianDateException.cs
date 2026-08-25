namespace Shared.Web.Exceptions;

public class InvalidPersianDateException(string? rawValue)
    : Exception($"مقدار «{rawValue}» یک تاریخ شمسی معتبر نیست. فرمت صحیح: yyyy/MM/dd");