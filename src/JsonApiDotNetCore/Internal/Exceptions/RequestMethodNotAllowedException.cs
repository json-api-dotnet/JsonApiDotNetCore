using System.Net;
using System.Net.Http;

namespace JsonApiDotNetCore.Internal
{
    public sealed class RequestMethodNotAllowedException : JsonApiException
    {
        public HttpMethod Method { get; }

        public RequestMethodNotAllowedException(HttpMethod method)
            : base(new Error
            {
                Status = HttpStatusCode.MethodNotAllowed,
                Title = "The request method is not allowed.",
                Detail = $"Resource does not support {method} requests."
            })
        {
            Method = method;
        }
    }
}
