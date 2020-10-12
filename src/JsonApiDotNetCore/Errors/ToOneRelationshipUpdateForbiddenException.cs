using System.Net;
using JsonApiDotNetCore.Serialization.Objects;

namespace JsonApiDotNetCore.Errors
{
    /// <summary>
    /// The error that is thrown when an attempt is made to update a to-one relationship on a to-many relationship endpoint.
    /// </summary>
    public sealed class ToOneRelationshipUpdateForbiddenException : JsonApiException
    {
        public ToOneRelationshipUpdateForbiddenException(string toOneRelationship)
            : base(new Error(HttpStatusCode.Forbidden)
            {
                Title = "The request to update the relationship is forbidden.",
                Detail = $"Relationship {toOneRelationship} is not a to-many relationship."
            }) { }
    }
}
