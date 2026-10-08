using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors;

/// <summary>
/// The error that is thrown when an operation in an atomic:operations request failed to be processed for unknown reasons.
/// </summary>
[PublicAPI]
public sealed class FailedOperationException(int? operationIndex, Exception innerException)
    : JsonApiException(new ErrorObject(HttpStatusCode.InternalServerError)
    {
        Title = operationIndex != null
            ? "An unhandled error occurred while processing an operation in this request."
            : "An unhandled error occurred while processing this operations request.",
        Detail = innerException.Message,
        Source = new ErrorSource
        {
            Pointer = operationIndex != null ? $"/atomic:operations[{operationIndex}]" : "/atomic:operations"
        }
    }, innerException);
