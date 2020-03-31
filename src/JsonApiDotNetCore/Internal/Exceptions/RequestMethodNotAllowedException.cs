using System.Net;
using System.Net.Http;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Internal
{
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
