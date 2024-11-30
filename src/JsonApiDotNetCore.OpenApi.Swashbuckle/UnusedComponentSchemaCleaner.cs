using System.Diagnostics;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Removes unreferenced component schemas from the OpenAPI document.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class UnusedComponentSchemaCleaner : IDocumentFilter
{
    private static readonly bool ThrowOnUnusedSchemaDetected = bool.Parse(bool.TrueString);

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        ArgumentGuard.NotNull(document);
        ArgumentGuard.NotNull(context);

        document.Components.Schemas.Remove(GenerationCacheSchemaGenerator.SchemaId);

        bool hasChanges;

        do
        {
            hasChanges = Cleanup(document);
        }
        while (hasChanges);
    }

    private static bool Cleanup(OpenApiDocument document)
    {
        var visitor = new OpenApiReferenceVisitor();
        var walker = new OpenApiWalker(visitor);
        walker.Walk(document);

        HashSet<string> unusedSchemaNames = [];

        foreach (string schemaId in document.Components.Schemas.Select(schema => schema.Key).Where(schemaId => !visitor.UsedSchemaNames.Contains(schemaId)))
        {
            unusedSchemaNames.Add(schemaId);
        }

        AssertNoUnknownSchemasFound(unusedSchemaNames);

        foreach (string schemaId in unusedSchemaNames)
        {
            document.Components.Schemas.Remove(schemaId);
        }

        return unusedSchemaNames.Count > 0;
    }

    [Conditional("DEBUG")]
    private static void AssertNoUnknownSchemasFound(HashSet<string> unusedSchemaNames)
    {
        if (ThrowOnUnusedSchemaDetected && unusedSchemaNames.Count > 0)
        {
            var remainingSchemaNames = new HashSet<string>(unusedSchemaNames);
            remainingSchemaNames.Remove(JsonApiPropertyName.Jsonapi);

            if (remainingSchemaNames.Count > 0)
            {
                throw new InvalidOperationException($"Detected unused component schemas: {string.Join(", ", remainingSchemaNames)}");
            }
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
