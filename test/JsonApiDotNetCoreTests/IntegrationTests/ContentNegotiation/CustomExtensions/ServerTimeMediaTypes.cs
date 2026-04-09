using JsonApiDotNetCore.Middleware;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal static class ServerTimeMediaTypes
{
    public static readonly JsonApiMediaType ServerTime = new([ServerTimeMediaTypeExtension.ServerTime]);

    public static readonly JsonApiMediaType AtomicOperationsWithServerTime = new([
        JsonApiMediaTypeExtension.AtomicOperations,
        ServerTimeMediaTypeExtension.ServerTime
    ]);
}
