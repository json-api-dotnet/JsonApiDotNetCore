using JsonApiDotNetCore.Middleware;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal static class ServerTimeMediaTypeExtension
{
    public static readonly JsonApiMediaTypeExtension ServerTime = new("https://www.jsonapi.net/ext/server-time");

    [Obsolete("This media type is no longer needed and will be removed in a future version. Use ServerTime instead.")]
    public static readonly JsonApiMediaTypeExtension RelaxedServerTime = new("server-time");
}
