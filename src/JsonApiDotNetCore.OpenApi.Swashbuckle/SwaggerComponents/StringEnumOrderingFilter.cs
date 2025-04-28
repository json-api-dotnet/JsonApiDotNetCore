using System.Text.Json;
using System.Text.Json.Nodes;
using JetBrains.Annotations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Services;
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
            if (schema is OpenApiSchema concreteSchema && HasSortAnnotation(concreteSchema))
            {
                if (schema.Enum != null && schema.Enum.Count > 1)
                {
                    OrderEnumMembers(concreteSchema);
                }
            }

            if (schema.Extensions != null)
            {
                schema.Extensions.Remove(RequiresSortKey);
            }
        }

        private static bool HasSortAnnotation(OpenApiSchema schema)
        {
            // Order our own enums, but don't touch enums from user-defined resource attributes.
            return schema.Extensions != null && schema.Extensions.TryGetValue(RequiresSortKey, out var extension) && extension is OpenApiAny any && any.Node is JsonValue value && value.GetValueKind() == JsonValueKind.True;
        }

        private static void OrderEnumMembers(OpenApiSchema schema)
        {
            var ordered = schema.Enum.OrderBy(node => node.ToString()).ToList();
            ConsistencyGuard.ThrowIf(ordered.Count != schema.Enum.Count);

            schema.Enum = ordered;
        }
    }
}
