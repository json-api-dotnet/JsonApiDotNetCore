using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi;

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

    public static OpenApiSchema UnwrapExtendedReferenceSchema(this OpenApiSchema source)
    {
        ArgumentGuard.NotNull(source);

        if (source.AllOf.Count != 1)
        {
            throw new InvalidOperationException($"Schema '{nameof(source)}' should not contain multiple entries in '{nameof(source.AllOf)}' ");
        }

        return source.AllOf.Single();
    }
}
