using JetBrains.Annotations;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi;

/// <summary>
/// Removes unreferenced component schemas from the OpenAPI document.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class UnusedComponentSchemaCleaner : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        var visitor = new OpenApiReferenceVisitor();
        var walker = new OpenApiWalker(visitor);
        walker.Walk(document);

        HashSet<string> unusedSchemaNames = [];

        foreach (string schemaId in document.Components.Schemas.Select(schema => schema.Key).Where(schemaId => !visitor.UsedSchemaNames.Contains(schemaId)))
        {
            unusedSchemaNames.Add(schemaId);
        }

        foreach (string schemaId in unusedSchemaNames)
        {
            document.Components.Schemas.Remove(schemaId);
        }
    }

    private sealed class OpenApiReferenceVisitor : OpenApiVisitorBase
    {
        private const string ComponentSchemaPrefix = "#/components/schemas/";

        public HashSet<string> UsedSchemaNames { get; } = [];

        public override void Visit(IOpenApiReferenceable referenceable)
        {
            UsedSchemaNames.Add(referenceable.Reference.Id);
        }

        public override void Visit(OpenApiSchema schema)
        {
            if (schema.Discriminator != null)
            {
                foreach (string discriminatorValue in schema.Discriminator.Mapping.Values)
                {
                    if (discriminatorValue.StartsWith(ComponentSchemaPrefix, StringComparison.Ordinal))
                    {
                        string schemaId = discriminatorValue[ComponentSchemaPrefix.Length..];
                        UsedSchemaNames.Add(schemaId);
                    }
                }
            }
        }
    }
}