using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Internal.Exceptions
{
    /// <summary>
    /// The error that is thrown when serializing the response body fails.
    /// </summary>
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
