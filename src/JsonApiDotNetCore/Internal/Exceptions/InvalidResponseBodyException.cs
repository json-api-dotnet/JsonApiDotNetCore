using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Internal.Exceptions
{
    public sealed class InvalidResponseBodyException : JsonApiException
    {
        public InvalidResponseBodyException(Exception innerException) : base(new Error(HttpStatusCode.InternalServerError)
        {
            Title = "Failed to serialize response body.",
            Detail = innerException.Message
        }, innerException)
        {
        }
    }
}
