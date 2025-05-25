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
            foreach (OpenApiSchema schema in document.Components.Schemas.Values.OfType<OpenApiSchema>())
            {
                if (schema.Extensions != null && schema.Extensions.ContainsKey(RequiresRootObjectTypeKey))
                {
                    schema.Type = JsonSchemaType.Object;
                    schema.Extensions.Remove(RequiresRootObjectTypeKey);
                }
            }
        }
    }
}
