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
            if (schema is OpenApiSchema { Extensions: not null } inlineSchema && HasSortAnnotation(inlineSchema.Extensions))
            {
                OrderEnumMembers(inlineSchema);
            }

            schema.Extensions?.Remove(RequiresSortKey);
        }

        private static bool HasSortAnnotation(IDictionary<string, IOpenApiExtension> schemaExtensions)
        {
            // Order our own enums, but don't touch enums from user-defined resource attributes.
            return schemaExtensions.TryGetValue(RequiresSortKey, out IOpenApiExtension? extension) &&
                extension is JsonNodeExtension { Node: JsonValue value } && value.GetValueKind() == JsonValueKind.True;
        }

        private static void OrderEnumMembers(OpenApiSchema inlineSchema)
        {
            if (inlineSchema.Enum is { Count: > 1 })
            {
                List<JsonNode> ordered = inlineSchema.Enum.OrderBy(node => node.ToString()).ToList();
                ConsistencyGuard.ThrowIf(ordered.Count != inlineSchema.Enum.Count);

                inlineSchema.Enum = ordered;
            }
        }
    }
}
