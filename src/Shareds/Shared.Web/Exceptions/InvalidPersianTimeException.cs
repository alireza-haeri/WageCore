namespace Shared.Web.Exceptions;

public class InvalidPersianTimeException(string? rawValue)
    : Exception($"مقدار «{rawValue}» یک ساعت معتبر نیست. فرمت صحیح: HH:mm");