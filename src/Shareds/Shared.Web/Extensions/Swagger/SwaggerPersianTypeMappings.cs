namespace Shared.Web.Extensions.Swagger;

public static class SwaggerPersianTypeMappings
{
    public static void AddPersianDateTimeMappings(this SwaggerGenOptions options)
    {
        options.MapType<PersianDate>(() => new OpenApiSchema { Type = JsonSchemaType.String });
        options.MapType<PersianTime>(() => new OpenApiSchema { Type = JsonSchemaType.String });
        options.MapType<PersianDateTime>(() => new OpenApiSchema { Type = JsonSchemaType.String });
    }
}