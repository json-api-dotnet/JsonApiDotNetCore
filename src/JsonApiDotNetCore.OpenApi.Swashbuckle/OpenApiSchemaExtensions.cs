using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class OpenApiSchemaExtensions
{
    public static void ReorderProperties(this OpenApiSchema fullSchema, IEnumerable<string> propertyNamesInOrder)
    {
        ArgumentNullException.ThrowIfNull(fullSchema);
        ArgumentNullException.ThrowIfNull(propertyNamesInOrder);

        var propertiesInOrder = new Dictionary<string, IOpenApiSchema>();

        foreach (var propertyName in propertyNamesInOrder)
        {
            if (fullSchema.Properties.TryGetValue(propertyName, out var schema))
            {
                propertiesInOrder.Add(propertyName, schema);
            }
        }

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
