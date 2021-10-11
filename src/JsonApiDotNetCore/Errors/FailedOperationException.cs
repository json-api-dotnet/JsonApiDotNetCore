#nullable disable

using System;
using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when an operation in an atomic:operations request failed to be processed for unknown reasons.
    /// </summary>
    [PublicAPI]
    public sealed class FailedOperationException : JsonApiException
    {
        public FailedOperationException(int operationIndex, Exception innerException)
            : base(new ErrorObject(HttpStatusCode.InternalServerError)
            {
                Title = "An unhandled error occurred while processing an operation in this request.",
                Detail = innerException.Message,
                Source = new ErrorSource
                {
                    Pointer = $"/atomic:operations[{operationIndex}]"
                }
            }, innerException)
        {
        }
    }
}
