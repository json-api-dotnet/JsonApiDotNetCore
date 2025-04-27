using System.Diagnostics;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Models.References;
using Microsoft.OpenApi.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

/// <summary>
/// Removes unreferenced component schemas from the OpenAPI document.
/// </summary>
[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class UnusedComponentSchemaCleaner : IDocumentFilter
{
    private const string ComponentSchemaPrefix = "#/components/schemas/";

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);

        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        document.Components.Schemas.Remove(GenerationCacheSchemaGenerator.SchemaId);

        var unusedSchemaIds = GetUnusedSchemaIds(document);
        AssertNoUnknownSchemasFound(unusedSchemaIds);

        RemoveUnusedComponentSchemas(document, unusedSchemaIds);
    }

    private static HashSet<string> GetUnusedSchemaIds(OpenApiDocument document)
    {
        var reachableSchemaIds = ReachableRootsCollector.Instance.CollectReachableSchemaIds(document);

        ComponentSchemaUsageCollector collector = new(document);
        return collector.CollectUnusedSchemaIds(reachableSchemaIds);
    }

    [Conditional("DEBUG")]
    private static void AssertNoUnknownSchemasFound(HashSet<string> unusedSchemaIds)
    {
        if (unusedSchemaIds.Count > 0)
        {
            throw new InvalidOperationException($"Detected unused component schemas: {string.Join(", ", unusedSchemaIds)}");
        }
    }

    private static void RemoveUnusedComponentSchemas(OpenApiDocument document, HashSet<string> unusedSchemaIds)
    {
        foreach (var schemaId in unusedSchemaIds)
        {
            document.Components ??= new OpenApiComponents();
            document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
            document.Components.Schemas.Remove(schemaId);
        }
    }

    private sealed class ReachableRootsCollector
    {
        public static ReachableRootsCollector Instance { get; } = new();

        private ReachableRootsCollector()
        {
        }

        public HashSet<string> CollectReachableSchemaIds(OpenApiDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            var visitor = new ComponentSchemaReferenceVisitor();

            var walker = new OpenApiWalker(visitor);
            walker.Walk(document);

            return visitor.ReachableSchemaIds;
        }

        private sealed class ComponentSchemaReferenceVisitor : OpenApiVisitorBase
        {
            public HashSet<string> ReachableSchemaIds { get; } = [];

            public override void Visit(IOpenApiReferenceHolder referenceHolder)
            {
                if (!PathString.StartsWith(ComponentSchemaPrefix, StringComparison.Ordinal))
                {
                    if (referenceHolder is OpenApiSchemaReference schema)
                    {
                        ReachableSchemaIds.Add(schema.Reference.Id);
                    }
                }
            }
        }
    }

    private sealed class ComponentSchemaUsageCollector
    {
        private readonly IDictionary<string, IOpenApiSchema> _componentSchemas;
        private readonly HashSet<string> _schemaIdsInUse = [];

        public ComponentSchemaUsageCollector(OpenApiDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            document.Components ??= new OpenApiComponents();
            document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
            _componentSchemas = document.Components.Schemas;
        }

        public HashSet<string> CollectUnusedSchemaIds(ICollection<string> reachableSchemaIds)
        {
            _schemaIdsInUse.Clear();

            foreach (var schemaId in reachableSchemaIds)
            {
                WalkSchemaId(schemaId);
            }

            var unusedSchemaIds = _componentSchemas.Keys.ToHashSet();
            unusedSchemaIds.ExceptWith(_schemaIdsInUse);
            return unusedSchemaIds;
        }

        private void WalkSchemaId(string schemaId)
        {
            if (_schemaIdsInUse.Add(schemaId))
            {
                if (_componentSchemas.TryGetValue(schemaId, out var schema))
                {
                    WalkSchema(schema);
                }
            }
        }

        private void WalkSchema(IOpenApiSchema? schema)
        {
            if (schema != null)
            {
                VisitSchema(schema);

                WalkSchema(schema.Items);
                WalkSchema(schema.Not);

                foreach (var subSchema in schema?.AllOf ?? [])
                {
                    WalkSchema(subSchema);
                }

                foreach (var subSchema in schema?.AnyOf ?? [])
                {
                    WalkSchema(subSchema);
                }

                foreach (var subSchema in schema?.OneOf ?? [])
                {
                    WalkSchema(subSchema);
                }

                foreach (var subSchema in schema?.Properties?.Values ?? [])
                {
                    WalkSchema(subSchema);
                }

                // ReSharper disable once TailRecursiveCall
                WalkSchema(schema?.AdditionalProperties);
            }
        }

        private void VisitSchema(IOpenApiSchema schema)
        {
            if (schema is OpenApiSchemaReference refSchema && refSchema.Reference is { Type: ReferenceType.Schema, IsExternal: false })
            {
                WalkSchemaId(refSchema.Reference.Id);
            }

            if (schema.Discriminator != null)
            {
                foreach (var mappingValue in schema.Discriminator.Mapping.Values)
                {
                    if (mappingValue.StartsWith(ComponentSchemaPrefix, StringComparison.Ordinal))
                    {
                        var schemaId = mappingValue[ComponentSchemaPrefix.Length..];
                        WalkSchemaId(schemaId);
                    }
                }
            }
        }
    }
}
