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
        public InvalidRequestBodyException(string reason)
            : this(reason, null, null)
        {
        }

        public InvalidRequestBodyException(string reason, string details)
            : this(reason, details, null)
        {
        }

        public InvalidRequestBodyException(Exception innerException)
            : this(null, null, innerException)
        {
        }

        private InvalidRequestBodyException(string reason, string details = null, Exception innerException = null)
            : base(new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = reason != null
                    ? "Failed to deserialize request body: " + reason
                    : "Failed to deserialize request body.",
                Detail = details ?? innerException?.Message
            }, innerException)
        {
        }
    }
}
