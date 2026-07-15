namespace SamarPlanner.Shared.Tests.Assertions;

public static class DomainResultAssertions
{
    public static T ShouldBeSuccess<T>(this DomainResult<T> result)
    {
        result.IsSuccess.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();

        var response = result.Response;
        response.Should().NotBeNull();

        return response;
    }

    public static string ShouldBeFailure<T>(this DomainResult<T> result, string? expectedMessagePart = null)
    {
        result.IsSuccess.Should().BeFalse();

        var errorMessage = result.ErrorMessage;
        errorMessage.Should().NotBeNullOrWhiteSpace();

        if (expectedMessagePart is not null)
            errorMessage.Should().Contain(expectedMessagePart);

        return errorMessage!;
    }
}