using JsonApiDotNetCore.Middleware;

#pragma warning disable AV1008 // Class should not be static

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal static class ServerTimeMediaTypeExtension
{
    public static readonly JsonApiMediaTypeExtension ServerTime = new("https://www.jsonapi.net/ext/server-time");
}
