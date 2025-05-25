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

        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        document.Components.Schemas.Remove(GenerationCacheSchemaGenerator.SchemaId);

        HashSet<string> unusedSchemaIds = GetUnusedSchemaIds(document);
        AssertNoUnknownSchemasFound(unusedSchemaIds);

        RemoveUnusedComponentSchemas(document, unusedSchemaIds);
    }

    private static HashSet<string> GetUnusedSchemaIds(OpenApiDocument document)
    {
        HashSet<string> reachableSchemaIds = ReachableRootsCollector.Instance.CollectReachableSchemaIds(document);

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
        foreach (string schemaId in unusedSchemaIds)
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
                    if (referenceHolder is OpenApiSchemaReference { Reference.Id: not null } schema)
                    {
                        ReachableSchemaIds.Add(schema.Reference.Id);
                    }
                }
            }
        }
    }

    private sealed class ComponentSchemaUsageCollector
    {
        private static readonly Dictionary<string, IOpenApiSchema>.ValueCollection EmptyValueCollection = new(new Dictionary<string, IOpenApiSchema>());
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

                foreach (IOpenApiSchema subSchema in schema.AllOf ?? [])
                {
                    WalkSchema(subSchema);
                }

                foreach (IOpenApiSchema subSchema in schema.AnyOf ?? [])
                {
                    WalkSchema(subSchema);
                }

                foreach (IOpenApiSchema subSchema in schema.OneOf ?? [])
                {
                    WalkSchema(subSchema);
                }

                foreach (IOpenApiSchema subSchema in schema.Properties?.Values ?? EmptyValueCollection)
                {
                    WalkSchema(subSchema);
                }

                // ReSharper disable once TailRecursiveCall
                WalkSchema(schema.AdditionalProperties);
            }
        }

        private void VisitSchema(IOpenApiSchema schema)
        {
            if (schema is OpenApiSchemaReference { Reference: { Type: ReferenceType.Schema, IsExternal: false } } refSchema)
            {
                WalkSchemaId(refSchema.Reference.Id);
            }

            if (schema.Discriminator is { Mapping: not null })
            {
                foreach (OpenApiSchemaReference mappingValue in schema.Discriminator.Mapping.Values)
                {
                    WalkSchemaId(mappingValue.Reference.Id);
                }
            }
        }
    }
}
