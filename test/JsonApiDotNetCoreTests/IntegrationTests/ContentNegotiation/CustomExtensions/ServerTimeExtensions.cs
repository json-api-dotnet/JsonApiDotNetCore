using JsonApiDotNetCore.Middleware;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal static class ServerTimeExtensions
{
    public static readonly JsonApiExtension ServerTime = new("https://www.jsonapi.net/ext/server-time");
    public static readonly JsonApiExtension RelaxedServerTime = new("server-time");
}
