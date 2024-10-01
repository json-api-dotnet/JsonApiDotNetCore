using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors;

/// <summary>
/// The error that is thrown when a request is received for an HTTP route that is not exposed.
/// </summary>
[PublicAPI]
public sealed class RouteNotAvailableException(HttpMethod method, string route)
    : JsonApiException(new ErrorObject(HttpStatusCode.Forbidden)
    {
        Title = "The requested endpoint is not accessible.",
        Detail = $"Endpoint '{route}' is not accessible for {method} requests."
    })
{
    public HttpMethod Method { get; } = method;
}
