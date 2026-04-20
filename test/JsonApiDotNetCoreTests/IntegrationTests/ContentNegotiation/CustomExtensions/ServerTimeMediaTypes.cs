using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal static class ServerTimeMediaTypes
{
    public static readonly JsonApiMediaType ServerTime = new([ServerTimeMediaTypeExtension.ServerTime]);

    [Obsolete("This media type is no longer needed and will be removed in a future version. Use ServerTime instead.")]
    public static readonly JsonApiMediaType RelaxedServerTime = new([ServerTimeMediaTypeExtension.RelaxedServerTime]);

    public static readonly JsonApiMediaType AtomicOperationsWithServerTime = new([
        JsonApiMediaTypeExtension.AtomicOperations,
        ServerTimeMediaTypeExtension.ServerTime
    ]);

    [Obsolete("This media type is no longer needed and will be removed in a future version. Use AtomicOperationsWithServerTime instead.")]
    public static readonly JsonApiMediaType RelaxedAtomicOperationsWithRelaxedServerTime = new([
        JsonApiMediaTypeExtension.RelaxedAtomicOperations,
        ServerTimeMediaTypeExtension.RelaxedServerTime
    ]);
}
