using Microsoft.OpenApi;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class OpenApiSchemaExtensions
{
    public static void ReorderProperties(this OpenApiSchema fullSchema, IEnumerable<string> propertyNamesInOrder)
    {
        ArgumentNullException.ThrowIfNull(fullSchema);
        ArgumentNullException.ThrowIfNull(propertyNamesInOrder);

        var propertiesInOrder = new Dictionary<string, IOpenApiSchema>();

        foreach (string propertyName in propertyNamesInOrder)
        {
            if (fullSchema.Properties != null && fullSchema.Properties.TryGetValue(propertyName, out IOpenApiSchema? schema))
            {
                propertiesInOrder.Add(propertyName, schema);
            }
        }

        ConsistencyGuard.ThrowIf(fullSchema.Properties == null);
        ConsistencyGuard.ThrowIf(fullSchema.Properties.Count != propertiesInOrder.Count);

        fullSchema.Properties = propertiesInOrder;
    }

    public static OpenApiSchema WrapInExtendedSchema(this IOpenApiSchema source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new OpenApiSchema
        {
            AllOf = [source]
        };
    }

    public static IOpenApiSchema UnwrapLastExtendedSchema(this IOpenApiSchema source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (source is OpenApiSchema && source.AllOf is { Count: > 0 })
        {
            return source.AllOf.Last();
        }

        return source;
    }
}
