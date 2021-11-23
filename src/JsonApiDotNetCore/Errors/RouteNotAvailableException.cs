using System.Net;
using System.Net.Http;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a request is received for an HTTP route that is not exposed.
    /// </summary>
    [PublicAPI]
    public sealed class RouteNotAvailableException : JsonApiException
    {
        public HttpMethod Method { get; }

        public RouteNotAvailableException(HttpMethod method, string route)
            : base(new ErrorObject(HttpStatusCode.Forbidden)
            {
                Title = "The requested endpoint is not accessible.",
                Detail = $"Endpoint '{route}' is not accessible for {method} requests."
            })
        {
            Method = method;
        }
    }
}
