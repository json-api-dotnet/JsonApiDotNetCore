using System;
using System.Net;
using System.Text;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when deserializing the request body fails.
    /// </summary>
    [PublicAPI]
    public sealed class InvalidRequestBodyException : JsonApiException
    {
        public string RequestBody { get; }

        public InvalidRequestBodyException(string reason, string details, string requestBody, string sourcePointer, Exception innerException = null)
            : base(new ErrorObject(HttpStatusCode.UnprocessableEntity)
            {
                Title = reason != null ? $"Failed to deserialize request body: {reason}" : "Failed to deserialize request body.",
                Detail = FormatErrorDetail(details, innerException),
                Source = sourcePointer == null
                    ? null
                    : new ErrorSource
                    {
                        Pointer = sourcePointer
                    }
            }, innerException)
        {
            RequestBody = requestBody;
        }

        private static string FormatErrorDetail(string details, Exception innerException)
        {
            var builder = new StringBuilder();
            builder.Append(details ?? innerException?.Message);

            return builder.Length > 0 ? builder.ToString() : null;
        }
    }
}
