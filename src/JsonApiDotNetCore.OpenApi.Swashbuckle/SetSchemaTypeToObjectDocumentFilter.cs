using JetBrains.Annotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
internal sealed class SetSchemaTypeToObjectDocumentFilter : IDocumentFilter
{
    internal const string RequiresRootObjectTypeKey = "x-requires-root-object-type";

    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        foreach (OpenApiSchema schema in document.Components.Schemas.Values)
        {
            if (schema.Extensions.ContainsKey(RequiresRootObjectTypeKey))
            {
                schema.Type = "object";
                schema.Extensions.Remove(RequiresRootObjectTypeKey);
            }
        }
    }
}
