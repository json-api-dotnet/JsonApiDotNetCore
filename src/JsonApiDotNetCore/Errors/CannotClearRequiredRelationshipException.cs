using System.Net;
using JetBrains.Annotations;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a required relationship is cleared.
    /// </summary>
    [PublicAPI]
    public sealed class CannotClearRequiredRelationshipException : JsonApiException
    {
        public CannotClearRequiredRelationshipException(string relationshipName, string resourceId, string resourceType)
            : base(new Error(HttpStatusCode.BadRequest)
            {
                Title = "Failed to clear a required relationship.",
                Detail = $"The relationship '{relationshipName}' of resource type '{resourceType}' " +
                    $"with ID '{resourceId}' cannot be cleared because it is a required relationship."
            })
        {
        }
    }
}
