namespace Shared.Web.Exceptions;

public class InvalidPersianDateTimeException(string? rawValue)
    : Exception($"مقدار «{rawValue}» یک تاریخ و ساعت معتبر نیست. فرمت صحیح: yyyy/MM/dd HH:mm");