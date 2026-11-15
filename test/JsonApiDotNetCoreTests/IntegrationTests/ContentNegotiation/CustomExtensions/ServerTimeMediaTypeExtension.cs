using JsonApiDotNetCore.Middleware;

namespace JsonApiDotNetCoreTests.IntegrationTests.ContentNegotiation.CustomExtensions;

internal static class ServerTimeMediaTypeExtension
{
    public static readonly JsonApiMediaTypeExtension ServerTime = new("https://www.jsonapi.net/ext/server-time");
}
