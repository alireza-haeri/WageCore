namespace Shared.Web.Extensions.Swagger;

public sealed class SummaryFromOperationIdFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (string.IsNullOrWhiteSpace(operation.Summary) && !string.IsNullOrWhiteSpace(operation.OperationId))
            operation.Summary = operation.OperationId;
    }
}