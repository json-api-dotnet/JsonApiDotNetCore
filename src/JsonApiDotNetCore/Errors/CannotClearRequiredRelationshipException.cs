using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors;

/// <summary>
/// The error that is thrown when a required relationship is cleared.
/// </summary>
[PublicAPI]
public sealed class CannotClearRequiredRelationshipException : JsonApiException
{
    public CannotClearRequiredRelationshipException(string relationshipName, string resourceType)
        : base(new ErrorObject(HttpStatusCode.BadRequest)
        {
            Title = "Failed to clear a required relationship.",
            Detail = $"The relationship '{relationshipName}' on resource type '{resourceType}' cannot be cleared because it is a required relationship."
        })
    {
    }
}
