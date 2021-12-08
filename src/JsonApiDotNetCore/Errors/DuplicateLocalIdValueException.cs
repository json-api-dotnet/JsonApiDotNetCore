using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors;

/// <summary>
/// The error that is thrown when assigning a local ID that was already assigned in an earlier operation.
/// </summary>
[PublicAPI]
public sealed class DuplicateLocalIdValueException : JsonApiException
{
    public DuplicateLocalIdValueException(string localId)
        : base(new ErrorObject(HttpStatusCode.BadRequest)
        {
            Title = "Another local ID with the same name is already defined at this point.",
            Detail = $"Another local ID with name '{localId}' is already defined at this point."
        })
    {
    }
}
