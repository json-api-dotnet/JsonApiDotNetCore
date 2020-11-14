using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when a required relationship is cleared.
    /// </summary>
    public sealed class CannotClearRequiredRelationshipException : JsonApiException
    {
        public CannotClearRequiredRelationshipException(string relationshipName, string resourceType) : base(new Error(HttpStatusCode.BadRequest)
        {
            Title = "A required relationship cannot be cleared.",
            Detail = $"The required relationship '{relationshipName}' of resource of type '{resourceType}' cannot be cleared."
        })
        {
        }
    }
}
