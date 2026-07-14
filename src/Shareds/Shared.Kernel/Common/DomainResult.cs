namespace Shared.Kernel.Common;

public class DomainResult
{
    protected DomainResult(bool isSuccess, string? errorMessage = null)
    {
        if(isSuccess && errorMessage is not null)
            throw new InvalidOperationException("Result is success");
        
        if(!isSuccess && errorMessage is null)
            throw new InvalidOperationException("Result is failure");
        
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
    
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public static DomainResult Success() => new DomainResult(true);
    public static DomainResult Failure(string errorMessage) => new DomainResult(false, errorMessage);
}

public class DomainResult<TResponse>(bool isSuccess, TResponse? response = default, string? errorMessage = null)
    : DomainResult(isSuccess, errorMessage)
{
    public TResponse? Response => IsSuccess ? response : throw new InvalidOperationException("Result is failure");

    public static DomainResult<TResponse> Success(TResponse value) => new DomainResult<TResponse>(true, value);
    public new static DomainResult<TResponse> Failure(string errorMessage) => new DomainResult<TResponse>(false, default, errorMessage);
}