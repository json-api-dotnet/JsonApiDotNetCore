using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal sealed class SortSchemasFilter : IDocumentFilter
{
    // Should use SwaggerGeneratorOptions.SchemaComparer
    private static readonly StringComparer DefaultStringComparer = StringComparer.Ordinal;

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Workaround for the change that ordering is no longer applied. See https://github.com/microsoft/OpenAPI.NET/issues/1314#issuecomment-2836828481.
        // Pending: https://github.com/microsoft/OpenAPI.NET/issues/2342

        swaggerDoc.Components ??= new OpenApiComponents();

        if (swaggerDoc.Components.Schemas != null)
        {
            swaggerDoc.Components.Schemas = new SortedDictionary<string, IOpenApiSchema>(swaggerDoc.Components.Schemas, DefaultStringComparer).ToDictionary();

            foreach (IOpenApiSchema schema in swaggerDoc.Components.Schemas.Values)
            {
                SortSchema(schema);
            }
        }
    }

    private static void SortSchema(IOpenApiSchema schema)
    {
        if (schema is OpenApiSchema concreteSchema)
        {
            if (concreteSchema.Required != null)
            {
                concreteSchema.Required = new SortedSet<string>(concreteSchema.Required, DefaultStringComparer).ToHashSet();
            }

            if (concreteSchema.Discriminator?.Mapping != null)
            {
                concreteSchema.Discriminator.Mapping =
                    new SortedDictionary<string, OpenApiSchemaReference>(concreteSchema.Discriminator.Mapping, DefaultStringComparer).ToDictionary();
            }

            if (concreteSchema.AllOf != null)
            {
                foreach (IOpenApiSchema subSchema in concreteSchema.AllOf)
                {
                    SortSchema(subSchema);
                }
            }
        }
    }
}
