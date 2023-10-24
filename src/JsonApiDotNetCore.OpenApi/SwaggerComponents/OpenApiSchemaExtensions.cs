using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi.SwaggerComponents;

internal static class OpenApiSchemaExtensions
{
    public static void ReorderProperties(this OpenApiSchema fullSchemaForResourceObject, IEnumerable<string> propertyNamesInOrder)
    {
        var propertiesInOrder = new Dictionary<string, OpenApiSchema>();

        foreach (string propertyName in propertyNamesInOrder)
        {
            if (fullSchemaForResourceObject.Properties.TryGetValue(propertyName, out OpenApiSchema? schema))
            {
                propertiesInOrder.Add(propertyName, schema);
            }
        }

        if (fullSchemaForResourceObject.Properties.Count != propertiesInOrder.Count)
        {
            throw new UnreachableCodeException();
        }

        fullSchemaForResourceObject.Properties = propertiesInOrder;
    }
}
