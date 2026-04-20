using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class OpenApiMediaTypes
{
    public static readonly JsonApiMediaType OpenApi = new([OpenApiMediaTypeExtension.OpenApi]);

    [Obsolete("This media type is no longer needed and will be removed in a future version. Use OpenApi instead.")]
    public static readonly JsonApiMediaType RelaxedOpenApi = new([OpenApiMediaTypeExtension.RelaxedOpenApi]);

    public static readonly JsonApiMediaType AtomicOperationsWithOpenApi = new([
        JsonApiMediaTypeExtension.AtomicOperations,
        OpenApiMediaTypeExtension.OpenApi
    ]);

    [Obsolete("This media type is no longer needed and will be removed in a future version. Use AtomicOperationsWithOpenApi instead.")]
    public static readonly JsonApiMediaType RelaxedAtomicOperationsWithRelaxedOpenApi = new([
        JsonApiMediaTypeExtension.RelaxedAtomicOperations,
        OpenApiMediaTypeExtension.RelaxedOpenApi
    ]);
}
