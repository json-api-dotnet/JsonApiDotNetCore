using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class OpenApiSchemaExtensions
{
    public static void ReorderProperties(this OpenApiSchema fullSchema, IEnumerable<string> propertyNamesInOrder)
    {
        ArgumentGuard.NotNull(fullSchema);
        ArgumentGuard.NotNull(propertyNamesInOrder);

        var propertiesInOrder = new Dictionary<string, OpenApiSchema>();

        foreach (string propertyName in propertyNamesInOrder)
        {
            if (fullSchema.Properties.TryGetValue(propertyName, out OpenApiSchema? schema))
            {
                propertiesInOrder.Add(propertyName, schema);
            }
        }

        if (fullSchema.Properties.Count != propertiesInOrder.Count)
        {
            throw new UnreachableCodeException();
        }

        fullSchema.Properties = propertiesInOrder;
    }

    public static OpenApiSchema WrapInExtendedSchema(this OpenApiSchema source)
    {
        ArgumentGuard.NotNull(source);

        return new OpenApiSchema
        {
            AllOf = [source]
        };
    }

    public static OpenApiSchema UnwrapLastExtendedSchema(this OpenApiSchema source)
    {
        ArgumentGuard.NotNull(source);

        if (source.AllOf is { Count: > 0 })
        {
            return source.AllOf.Last();
        }

        return source;
    }
}
