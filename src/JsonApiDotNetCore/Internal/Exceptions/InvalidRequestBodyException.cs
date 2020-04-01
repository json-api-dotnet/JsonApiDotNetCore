using System;
using System.Net;
using System.Net.Http;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Internal
{
    public sealed class InvalidRequestBodyException : JsonApiException
    {
        public InvalidRequestBodyException(string message, Exception innerException = null)
            : base(new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = message ?? "Failed to deserialize request body.",
                Detail = innerException?.Message
            }, innerException)
        {
        }
    }
}
