using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal static class ServerTimeMediaTypes
{
    public static readonly JsonApiMediaType ServerTime = new([ServerTimeMediaTypeExtension.ServerTime]);

    public static readonly JsonApiMediaType AtomicOperationsWithServerTime = new([
        JsonApiMediaTypeExtension.AtomicOperations,
        ServerTimeMediaTypeExtension.ServerTime
    ]);
}
