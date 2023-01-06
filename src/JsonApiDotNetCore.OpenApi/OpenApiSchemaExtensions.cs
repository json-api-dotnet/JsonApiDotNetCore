using Microsoft.OpenApi.Models;

namespace JsonApiDotNetCore.OpenApi;

internal static class OpenApiSchemaExtensions
{
    public static OpenApiSchema UnwrapExtendedReferenceSchema(this OpenApiSchema source)
    {
        ArgumentGuard.NotNull(source);

        if (source.AllOf.Count != 1)
        {
            throw new InvalidOperationException($"Schema '{nameof(source)}' should not contain multiple entries in '{nameof(source.AllOf)}' ");
        }

        return source.AllOf.Single();
    }
}
