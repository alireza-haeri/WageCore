namespace SamarPlanner.Shared.Tests.Assertions;

public static class HttpResponseAssertions
{
    public static async Task<TResponse> ShouldBeSuccess<TResponse>(this HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await ReadResultFromResponse<TResponse>(response);
        
        body.Response.Should().NotBeNull();
        body.IsSuccess.Should().BeTrue();
        body.BadResultType.Should().BeNull();
        body.Errors.Should().BeNull();
        
        return body.Response;
    }
    
    public static async Task<TResponse> ShouldBeFailure<TResponse>(this HttpResponseMessage response,BadResultType? expectedBadResultType = null)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var body = await ReadResultFromResponse<TResponse>(response);
        
        body.Response.Should().BeNull();
        body.IsSuccess.Should().BeFalse();
        body.BadResultType.Should().NotBeNull();
        body.Errors.Should().NotBeNull();
        
        if(expectedBadResultType != null)
            body.BadResultType.Should().Be(expectedBadResultType);
        
        return body.Response;
    }
    
    private static async Task<Result<TResponse>> ReadResultFromResponse<TResponse>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<Result<TResponse>>();
        body.Should().NotBeNull();
        return body;
    }
}