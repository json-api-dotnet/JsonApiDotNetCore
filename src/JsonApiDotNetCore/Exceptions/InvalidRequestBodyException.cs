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
        public InvalidRequestBodyException(string reason, string details, string requestBody, Exception innerException = null)
            : base(new Error(HttpStatusCode.UnprocessableEntity)
            {
                Title = reason != null
                    ? "Failed to deserialize request body: " + reason
                    : "Failed to deserialize request body.",
                Detail = FormatDetails(details, requestBody, innerException)
            }, innerException)
        {
        }

        private static string FormatDetails(string details, string requestBody, Exception innerException)
        {
            string text = details ?? innerException?.Message;

            if (requestBody != null)
            {
                if (text != null)
                {
                    text += " - ";
                }

                text += "Request body: <<" + requestBody + ">>";
            }

            return text;
        }
    }
}
