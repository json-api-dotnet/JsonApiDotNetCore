using System.Diagnostics;
using JetBrains.Annotations;
using JsonApiDotNetCore.OpenApi.Swashbuckle.SchemaGenerators;
using Microsoft.OpenApi;
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

        if (document.Components?.Schemas != null)
        {
            document.Components.Schemas.Remove(GenerationCacheSchemaGenerator.SchemaId);

            HashSet<string> unusedSchemaIds = GetUnusedSchemaIds(document, document.Components.Schemas);
            AssertNoUnknownSchemasFound(unusedSchemaIds);

            RemoveUnusedComponentSchemas(document.Components.Schemas, unusedSchemaIds);
        }
    }

    private static HashSet<string> GetUnusedSchemaIds(OpenApiDocument document, IDictionary<string, IOpenApiSchema> componentSchemas)
    {
        HashSet<string> reachableSchemaIds = ReachableRootsCollector.Instance.CollectReachableSchemaIds(document);

        ComponentSchemaUsageCollector collector = new(componentSchemas);
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

    private static void RemoveUnusedComponentSchemas(IDictionary<string, IOpenApiSchema> componentSchemas, HashSet<string> unusedSchemaIds)
    {
        foreach (string schemaId in unusedSchemaIds)
        {
            componentSchemas.Remove(schemaId);
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
                    if (referenceHolder is OpenApiSchemaReference { Reference.Id: not null } referenceSchema)
                    {
                        ReachableSchemaIds.Add(referenceSchema.Reference.Id);
                    }
                }
            }
        }
    }

    private sealed class ComponentSchemaUsageCollector
    {
        private readonly IDictionary<string, IOpenApiSchema> _componentSchemas;
        private readonly HashSet<string> _schemaIdsInUse = [];

        public ComponentSchemaUsageCollector(IDictionary<string, IOpenApiSchema> componentSchemas)
        {
            ArgumentNullException.ThrowIfNull(componentSchemas);

            _componentSchemas = componentSchemas;
        }

        public HashSet<string> CollectUnusedSchemaIds(ICollection<string> reachableSchemaIds)
        {
            _schemaIdsInUse.Clear();

            foreach (string schemaId in reachableSchemaIds)
            {
                WalkSchemaId(schemaId);
            }

            HashSet<string> unusedSchemaIds = _componentSchemas.Keys.ToHashSet();
            unusedSchemaIds.ExceptWith(_schemaIdsInUse);
            return unusedSchemaIds;
        }

        private void WalkSchemaId(string? schemaId)
        {
            if (schemaId != null && _schemaIdsInUse.Add(schemaId))
            {
                if (_componentSchemas.TryGetValue(schemaId, out IOpenApiSchema? schema))
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

                foreach (IOpenApiSchema subSchema in schema.AllOf ?? Array.Empty<IOpenApiSchema>())
                {
                    WalkSchema(subSchema);
                }

                foreach (IOpenApiSchema subSchema in schema.AnyOf ?? Array.Empty<IOpenApiSchema>())
                {
                    WalkSchema(subSchema);
                }

                foreach (IOpenApiSchema subSchema in schema.OneOf ?? Array.Empty<IOpenApiSchema>())
                {
                    WalkSchema(subSchema);
                }

                foreach (IOpenApiSchema subSchema in schema.Properties?.Values ?? Array.Empty<IOpenApiSchema>())
                {
                    WalkSchema(subSchema);
                }

                // ReSharper disable once TailRecursiveCall
                WalkSchema(schema.AdditionalProperties);
            }
        }

        private void VisitSchema(IOpenApiSchema schema)
        {
            if (schema is OpenApiSchemaReference { Reference: { Type: ReferenceType.Schema, IsExternal: false } } referenceSchema)
            {
                WalkSchemaId(referenceSchema.Reference.Id);
            }

            if (schema.Discriminator is { Mapping: not null })
            {
                foreach (OpenApiSchemaReference mappingValueReferenceSchema in schema.Discriminator.Mapping.Values)
                {
                    WalkSchemaId(mappingValueReferenceSchema.Reference.Id);
                }
            }
        }
    }
}
