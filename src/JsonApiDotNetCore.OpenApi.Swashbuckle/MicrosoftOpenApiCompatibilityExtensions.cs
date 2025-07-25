using Microsoft.OpenApi;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class MicrosoftOpenApiCompatibilityExtensions
{
    public static void SetNullable(this OpenApiSchema schema, bool nullable)
    {
        ArgumentNullException.ThrowIfNull(schema);

        if (nullable)
        {
            schema.Type ??= JsonSchemaType.Null;
            schema.Type |= JsonSchemaType.Null;
        }
        else
        {
            if (schema.Type != null)
            {
                schema.Type &= ~JsonSchemaType.Null;
            }
        }
    }
}
