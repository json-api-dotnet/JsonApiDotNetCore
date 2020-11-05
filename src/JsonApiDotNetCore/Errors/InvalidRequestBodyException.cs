using System;
using System.Net;
using System.Text;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
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
                Detail = FormatErrorDetail(details, requestBody, innerException)
            }, innerException)
        {
        }

        private static string FormatErrorDetail(string details, string requestBody, Exception innerException)
        {
            var builder = new StringBuilder();
            builder.Append(details ?? innerException?.Message);

            if (requestBody != null)
            {
                if (builder.Length > 0)
                {
                    builder.Append(" - ");
                }

                builder.Append("Request body: <<");
                builder.Append(requestBody);
                builder.Append(">>");
            }

            return builder.Length > 0 ? builder.ToString() : null;
        }
    }
}
