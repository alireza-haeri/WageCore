namespace Shared.Kernel.Common;

public class Result<TModel>
{
    public bool IsSuccess { get; init; }
    public BadResultType? BadResultType { get; init; }
    public TModel? Response { get; init; }
    public IDictionary<string, string[]>? Errors { get; init; }

    public static Result<TModel> Success(TModel response) =>
        new() { IsSuccess = true, Response = response };

    public static Result<TModel> GeneralFailure(string generalError = "خطایی رخ داده است!") =>
        new()
        {
            IsSuccess = false,
            Errors = new Dictionary<string, string[]>
                { { "General", [generalError] } },
            BadResultType =  Common.BadResultType.General
        };

    public static Result<TModel> ValidationFailure(IDictionary<string, string[]> validationErrors) =>
        new() { IsSuccess = false, Errors = validationErrors, BadResultType = Common.BadResultType.Validation };

    public static Result<TModel> NotfoundFailure(string message, string? key = null) =>
        new()
        {
            IsSuccess = false,
            Errors = new Dictionary<string, string[]> { { key ?? "General", [message] } },
            BadResultType =  Common.BadResultType.NotFound
        };

    public static Result<TModel> UnAuthorizeFailure()=>
        new()
        {
            IsSuccess = false,
            Errors = new Dictionary<string, string[]>
                { { "General", ["شما دسترسی لازم را ندارید. لطفاً وارد شوید."] } },
            BadResultType =  Common.BadResultType.Unauthorized
        };
}

public enum BadResultType
{
    NotFound = 0,
    Validation = 1,
    General = 2,
    Unauthorized = 3,
}