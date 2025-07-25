using System.Text.Json;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle.SwaggerComponents;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class StringEnumOrderingFilter : IDocumentFilter
{
    internal const string RequiresSortKey = "x-requires-sort";

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);

        var visitor = new OpenApiEnumVisitor();
        var walker = new OpenApiWalker(visitor);
        walker.Walk(document);
    }

    private sealed class OpenApiEnumVisitor : OpenApiVisitorBase
    {
        public override void Visit(IOpenApiSchema schema)
        {
            if (schema is OpenApiSchema { Extensions: not null } concreteSchema && HasSortAnnotation(concreteSchema.Extensions))
            {
                OrderEnumMembers(concreteSchema);
            }

            schema.Extensions?.Remove(RequiresSortKey);
        }

        private static bool HasSortAnnotation(IDictionary<string, IOpenApiExtension> schemaExtensions)
        {
            // Order our own enums, but don't touch enums from user-defined resource attributes.
            return schemaExtensions.TryGetValue(RequiresSortKey, out IOpenApiExtension? extension) &&
                extension is JsonNodeExtension { Node: JsonValue value } && value.GetValueKind() == JsonValueKind.True;
        }

        private static void OrderEnumMembers(OpenApiSchema schema)
        {
            if (schema.Enum is { Count: > 1 })
            {
                List<JsonNode> ordered = schema.Enum.OrderBy(node => node.ToString()).ToList();
                ConsistencyGuard.ThrowIf(ordered.Count != schema.Enum.Count);

                schema.Enum = ordered;
            }
        }
    }
}
