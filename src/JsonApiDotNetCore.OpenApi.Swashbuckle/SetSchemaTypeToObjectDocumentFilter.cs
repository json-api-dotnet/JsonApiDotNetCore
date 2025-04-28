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
        if (document.Components?.Schemas != null)
        {
            foreach (var schema in document.Components.Schemas.Values)
            {
                if (schema.Extensions != null && schema.Extensions.ContainsKey(RequiresRootObjectTypeKey))
                {
                    ((OpenApiSchema)schema).Type = JsonSchemaType.Object;
                    schema.Extensions.Remove(RequiresRootObjectTypeKey);
                }
            }
        }
    }
}
