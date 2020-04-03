using System;
using System.Net;
using JsonApiDotNetCore.Models.JsonApiDocuments;

namespace JsonApiDotNetCore.Exceptions
{
    /// <summary>
    /// The error that is thrown when deserializing the request body fails.
    /// </summary>
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
