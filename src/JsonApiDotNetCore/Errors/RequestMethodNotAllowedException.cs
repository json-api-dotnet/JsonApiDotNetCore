using System.Net;
using System.Net.Http;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a request is received that contains an unsupported HTTP verb.
    /// </summary>
    [PublicAPI]
    public sealed class RequestMethodNotAllowedException : JsonApiException
    {
        public HttpMethod Method { get; }

        public RequestMethodNotAllowedException(HttpMethod method)
            : base(new Error(HttpStatusCode.MethodNotAllowed)
            {
                Title = "The request method is not allowed.",
                Detail = $"Resource does not support {method} requests."
            })
        {
            Method = method;
        }
    }
}
