using Microsoft.OpenApi;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class OpenApiSchemaExtensions
{
    public static OpenApiSchema AsInlineSchema(this IOpenApiSchema schema)
    {
        ConsistencyGuard.ThrowIf(schema is not OpenApiSchema);
        return (OpenApiSchema)schema;
    }

    public static OpenApiSchemaReference AsReferenceSchema(this IOpenApiSchema schema)
    {
        ConsistencyGuard.ThrowIf(schema is not OpenApiSchemaReference);
        return (OpenApiSchemaReference)schema;
    }

    public static string GetReferenceId(this OpenApiSchemaReference referenceSchema)
    {
        string? schemaId = referenceSchema.Reference.Id;
        ConsistencyGuard.ThrowIf(schemaId is null);
        return schemaId;
    }

    public static void SetNullable(this OpenApiSchema inlineSchema, bool nullable)
    {
        ArgumentNullException.ThrowIfNull(inlineSchema);

        if (nullable)
        {
            inlineSchema.Type ??= JsonSchemaType.Null;
            inlineSchema.Type |= JsonSchemaType.Null;
        }
        else
        {
            if (inlineSchema.Type != null)
            {
                inlineSchema.Type &= ~JsonSchemaType.Null;
            }
        }
    }

    public static void ReorderProperties(this OpenApiSchema inlineSchema, IEnumerable<string> propertyNamesInOrder)
    {
        ArgumentNullException.ThrowIfNull(inlineSchema);
        ArgumentNullException.ThrowIfNull(propertyNamesInOrder);

        if (inlineSchema.Properties is { Count: > 1 })
        {
            var propertiesInOrder = new Dictionary<string, IOpenApiSchema>();

            foreach (string propertyName in propertyNamesInOrder)
            {
                if (inlineSchema.Properties.TryGetValue(propertyName, out IOpenApiSchema? schema))
                {
                    propertiesInOrder.Add(propertyName, schema);
                }
            }

            ConsistencyGuard.ThrowIf(inlineSchema.Properties.Count != propertiesInOrder.Count);

            inlineSchema.Properties = propertiesInOrder;
        }
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
