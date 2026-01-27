using JetBrains.Annotations;
using Microsoft.OpenApi;
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
            foreach (OpenApiSchema inlineSchema in document.Components.Schemas.Values.OfType<OpenApiSchema>())
            {
                if (inlineSchema.Extensions != null && inlineSchema.Extensions.ContainsKey(RequiresRootObjectTypeKey))
                {
                    inlineSchema.Type = JsonSchemaType.Object;
                    inlineSchema.Extensions.Remove(RequiresRootObjectTypeKey);
                }
            }
        }
    }
}
