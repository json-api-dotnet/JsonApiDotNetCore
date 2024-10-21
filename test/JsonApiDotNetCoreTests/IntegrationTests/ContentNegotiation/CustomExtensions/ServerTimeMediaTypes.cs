using JsonApiDotNetCore.Middleware;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal static class ServerTimeMediaTypes
{
    public static readonly JsonApiMediaType ServerTime = new([ServerTimeExtensions.ServerTime]);
    public static readonly JsonApiMediaType RelaxedServerTime = new([ServerTimeExtensions.RelaxedServerTime]);

    public static readonly JsonApiMediaType AtomicOperationsWithServerTime = new([
        JsonApiExtension.AtomicOperations,
        ServerTimeExtensions.ServerTime
    ]);

    public static readonly JsonApiMediaType RelaxedAtomicOperationsWithRelaxedServerTime = new([
        JsonApiExtension.RelaxedAtomicOperations,
        ServerTimeExtensions.RelaxedServerTime
    ]);
}
