namespace SamarPlanner.Shared.Tests.Assertions;

public static class ResultAssertions
{
    public static T ShouldBeSuccess<T>(this Result<T> result)
    {
        result.IsSuccess.Should().BeTrue();
        result.Errors.Should().BeNull();

        var response = result.Response;
        response.Should().NotBeNull();

        return response;
    }

    public static IDictionary<string, string[]>? ShouldBeFailure<T>(this Result<T> result,
        string? expectedMessagePart = null,BadResultType? expectedResultType = null)
    {
        result.IsSuccess.Should().BeFalse();

        var errorMessage = result.Errors;
        errorMessage.Should().NotBeNull();

        if (expectedMessagePart is not null)
            errorMessage.Any(e => e.Value.Contains(expectedMessagePart)).Should().BeTrue();

        if (expectedResultType is not null)
            result.BadResultType.Should().Be(expectedResultType);

        return errorMessage;
    }
}