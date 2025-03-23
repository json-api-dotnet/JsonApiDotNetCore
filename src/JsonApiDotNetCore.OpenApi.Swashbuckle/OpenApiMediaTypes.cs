using JsonApiDotNetCore.Middleware;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCore.OpenApi.Swashbuckle;

internal static class OpenApiMediaTypes
{
    public static readonly JsonApiMediaType OpenApi = new([OpenApiMediaTypeExtension.OpenApi]);
    public static readonly JsonApiMediaType RelaxedOpenApi = new([OpenApiMediaTypeExtension.RelaxedOpenApi]);

    public static readonly JsonApiMediaType AtomicOperationsWithOpenApi = new([
        JsonApiMediaTypeExtension.AtomicOperations,
        OpenApiMediaTypeExtension.OpenApi
    ]);

    public static readonly JsonApiMediaType RelaxedAtomicOperationsWithRelaxedOpenApi = new([
        JsonApiMediaTypeExtension.RelaxedAtomicOperations,
        OpenApiMediaTypeExtension.RelaxedOpenApi
    ]);
}
