using JetBrains.Annotations;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
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
        public override void Visit(OpenApiSchema schema)
        {
            if (schema.Enum.Count > 0)
            {
                if (HasSortAnnotation(schema))
                {
                    if (schema.Enum.Count > 1)
                    {
                        OrderEnumMembers(schema);
                    }
                }

                schema.Extensions.Remove(RequiresSortKey);
            }
        }

        private static bool HasSortAnnotation(OpenApiSchema schema)
        {
            // Order our own enums, but don't touch enums from user-defined resource attributes.
            return schema.Extensions.TryGetValue(RequiresSortKey, out IOpenApiExtension? extension) && extension is OpenApiBoolean { Value: true };
        }

        private static void OrderEnumMembers(OpenApiSchema schema)
        {
            List<IOpenApiAny> ordered = schema.Enum.OfType<OpenApiString>().OrderBy(openApiString => openApiString.Value).Cast<IOpenApiAny>().ToList();

            if (ordered.Count != schema.Enum.Count)
            {
                throw new UnreachableCodeException();
            }

            schema.Enum = ordered;
        }
    }
}
