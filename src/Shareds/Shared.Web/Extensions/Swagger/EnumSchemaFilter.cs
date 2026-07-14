namespace Shared.Web.Extensions.Swagger;

public sealed class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        var type = Nullable.GetUnderlyingType(context.Type) ?? context.Type;

        if (type == null || !type.IsEnum)
            return;

        if(schema is not OpenApiSchema openApiSchema)
            return;

        var names = new JsonArray();
        foreach (var name in Enum.GetNames(type))
            names.Add(JsonValue.Create(name));

        openApiSchema.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        openApiSchema.Extensions["x-enumNames"] = new JsonNodeExtension(names);
        openApiSchema.Extensions["x-enum-varnames"] = new JsonNodeExtension(names);
    }
}